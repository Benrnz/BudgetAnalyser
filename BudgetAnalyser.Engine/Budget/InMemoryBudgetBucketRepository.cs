using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Budget.Data;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A thread-safe in memory implementation of <see cref="IBudgetBucketRepository" />.
    ///     This repository does not need to be persisted, because it uses the <see cref="BudgetCollection" /> as the source of
    ///     truth. Thread safety is built in to allow multiple threads to load data from statements asynchronously.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryBudgetBucketRepository : IBudgetBucketRepository
    {
        private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> mapper;
        private readonly object syncRoot = new object();
        private Dictionary<string, BudgetBucket> lookupTable = new Dictionary<string, BudgetBucket>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="InMemoryBudgetBucketRepository" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public InMemoryBudgetBucketRepository([NotNull] IDtoMapper<BudgetBucketDto, BudgetBucket> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            this.mapper = mapper;
        }

        /// <summary>
        ///     Gets all known budget buckets.
        /// </summary>
        public virtual IEnumerable<BudgetBucket> Buckets => this.lookupTable.Values.OrderBy(b => b.Code).ToList();

        /// <summary>
        ///     Gets the surplus bucket. This is for convenience only, it also exists in the <see cref="Buckets" /> collection
        /// </summary>
        public BudgetBucket SurplusBucket { get; protected set; }

        /// <summary>
        ///     Creates the new fixed budget project.
        /// </summary>
        /// <param name="bucketCode">The bucket code.</param>
        /// <param name="description">The description.</param>
        /// <param name="fixedBudgetAmount">The fixed budget amount.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The fixed budget amount must be greater than zero.
        ///     or
        ///     A new fixed budget project bucket cannot be created, because the code  + bucketCode +  already exists.
        /// </exception>
        public FixedBudgetProjectBucket CreateNewFixedBudgetProject(string bucketCode, string description,
                                                                    decimal fixedBudgetAmount)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException(nameof(bucketCode));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (fixedBudgetAmount <= 0)
            {
                throw new ArgumentException("The fixed budget amount must be greater than zero.",
                    nameof(fixedBudgetAmount));
            }

            var upperCode = FixedBudgetProjectBucket.CreateCode(bucketCode);
            if (IsValidCode(upperCode))
            {
                throw new ArgumentException(
                    "A new fixed budget project bucket cannot be created, because the code " + bucketCode +
                    " already exists.", bucketCode);
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(upperCode))
                {
                    throw new ArgumentException(
                        "A new fixed budget project bucket cannot be created, because the code " + bucketCode +
                        " already exists.", bucketCode);
                }

                var bucket = new FixedBudgetProjectBucket(bucketCode, description, fixedBudgetAmount);
                this.lookupTable.Add(upperCode, bucket);
                return bucket;
            }
        }

        /// <summary>
        ///     Gets a bucket by its code.
        /// </summary>
        /// <param name="code">The code, also used as a key and must be unique.</param>
        /// <returns>
        ///     The registered bucket or null if the given code doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual BudgetBucket GetByCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var upperCode = code.ToUpperInvariant();
            if (IsValidCode(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            return null;
        }

        /// <summary>
        ///     Gets the bucket by its code or creates a new one if not found.
        /// </summary>
        /// <param name="code">The code, also used as a key and must be unique.</param>
        /// <param name="factory">The factory to create the new bucket if not already registered.</param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public virtual BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var upperCode = code.ToUpperInvariant();
            if (ContainsKeyInternal(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            lock (this.syncRoot)
            {
                if (ContainsKeyInternal(upperCode))
                {
                    return this.lookupTable[upperCode];
                }

                var newBucket = factory();
                this.lookupTable.Add(upperCode, newBucket);
                return newBucket;
            }
        }

        /// <summary>
        ///     Initialises the buckets from the provided data.  Used by persistence.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void Initialise(IEnumerable<BudgetBucketDto> buckets)
        {
            if (buckets == null)
            {
                throw new ArgumentNullException(nameof(buckets));
            }

            lock (this.syncRoot)
            {
                this.lookupTable = buckets
                    .Where(dto => dto.Type != BucketDtoType.PayCreditCard && dto.Type != BucketDtoType.Surplus)
                    .Select(this.mapper.ToModel)
                    .Distinct()
                    .ToDictionary(e => e.Code, e => e);

                InitialiseMandatorySpecialBuckets();
            }
        }

        /// <summary>
        ///     Determines whether the bucket code is registered in this repository.
        /// </summary>
        /// <param name="code">The code, also used as a key and must be unique.</param>
        /// <returns>
        ///     True if found, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool IsValidCode(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            return ContainsKeyInternal(code);
        }

        /// <summary>
        ///     Removes the fixed budget project bucket permanently
        /// </summary>
        /// <param name="projectBucket">The project bucket to remove.</param>
        public void RemoveFixedBudgetProject(FixedBudgetProjectBucket projectBucket)
        {
            if (projectBucket == null)
            {
                throw new ArgumentNullException(nameof(projectBucket));
            }

            lock (this.syncRoot)
            {
                if (!IsValidCode(projectBucket.Code))
                {
                    throw new ArgumentException(
                        "Unable to remove the fixed project bucket, it does not appear to exist in the bucket repository.");
                }

                if (!this.lookupTable.Remove(projectBucket.Code))
                {
                    throw new InvalidOperationException(
                        "Unable to remove the fixed project bucket, removal from the internal dictionary failed. " +
                        projectBucket.Code);
                }
            }
        }

        /// <summary>
        ///     Adds the bucket.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        protected void AddBucket([NotNull] BudgetBucket bucket)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException(nameof(bucket));
            }

            if (IsValidCode(bucket.Code))
            {
                return;
            }

            lock (this.syncRoot)
            {
                if (IsValidCode(bucket.Code))
                {
                    return;
                }

                this.lookupTable.Add(bucket.Code, bucket);
            }
        }

        /// <summary>
        ///     Determines whether [contains key internal] [the specified code].
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
            Justification = "Already validated upstream")]
        protected bool ContainsKeyInternal(string code)
        {
            return this.lookupTable.ContainsKey(code.ToUpperInvariant());
        }

        /// <summary>
        ///     Initialises the mandatory special buckets.
        /// </summary>
        protected void InitialiseMandatorySpecialBuckets()
        {
            SurplusBucket = new SurplusBucket();
            AddBucket(SurplusBucket);
            AddBucket(new PayCreditCardBucket(PayCreditCardBucket.PayCreditCardCode,
                "A special bucket to allocate internal transfers."));
        }
    }
}