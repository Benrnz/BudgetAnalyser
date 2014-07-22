using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class InMemoryBudgetBucketRepositoryTest
    {
        private bool concurrencyFail;

        private static bool isAutoMapperConfigured;

        [TestMethod]
        public void AfterInitialiseJournalBucketShouldExist()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(new List<BudgetBucketDto>());

            Assert.IsTrue(subject.IsValidCode(JournalBucket.JournalCode));
            Assert.IsInstanceOfType(subject.GetByCode(JournalBucket.JournalCode), typeof(JournalBucket));
        }

        [TestMethod]
        public void AfterInitialiseSurplusBucketShouldExist()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(new List<BudgetBucketDto>());

            Assert.IsTrue(subject.IsValidCode(SurplusBucket.SurplusCode));
            Assert.IsInstanceOfType(subject.GetByCode(SurplusBucket.SurplusCode), typeof(SurplusBucket));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMapper()
        {
            new InMemoryBudgetBucketRepository(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetByCodeShouldThrowGivenNullCode()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            subject.GetByCode(null);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetByOrCreateNewShouldThrowGivenNullCode()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            subject.GetOrCreateNew(null, () => new JournalBucket());

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetByOrCreateNewShouldThrowGivenNullFactory()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            subject.GetOrCreateNew("CODE", null);

            Assert.Fail();
        }

        [TestMethod]
        public void GetOrAddShouldAddWhenItemDoesntExist()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            subject.GetOrCreateNew("Foo", () => new IncomeBudgetBucket("Foo", "Bar"));

            Assert.IsTrue(subject.IsValidCode("Foo"));
        }

        [TestMethod]
        public void GetOrAddShouldNotAddWhenItemDoesExist()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            int count = subject.Buckets.Count();
            subject.GetOrCreateNew(TestDataConstants.HairBucketCode, () =>
            {
                Assert.Fail();
                return new IncomeBudgetBucket(TestDataConstants.HairBucketCode, "Bar");
            });

            Assert.AreEqual(count, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulate9Buckets()
        {
            InMemoryBudgetBucketRepository subject = Arrange();
            int expected = CreateBudgetBucketDtoTestData().Count() + 2; // Surplus and Journal are added automatically.
            Assert.AreEqual(expected, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulateKnownBuckets()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            Assert.IsTrue(subject.IsValidCode(TestDataConstants.CarMtcBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.HairBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.FoodBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.IncomeBucketCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitialiseShouldThrowGivenNullBucketsArgument()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(null);

            Assert.Fail();
        }

        [TestMethod]
        public void IsValidCodeShouldReturnFalseWhenRepositoryIsEmpty()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            Assert.IsFalse(subject.IsValidCode(SurplusBucket.SurplusCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsValidCodeShouldThrowGivenNullCode()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            subject.IsValidCode(null);

            Assert.Fail();
        }

        [TestMethod]
        public void NewRepositoryShouldBeEmpty()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            Assert.IsFalse(subject.Buckets.Any());
        }

        [TestMethod]
        public void NewRepositoryShouldNotContainDefaultBuckets()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            Assert.IsNull(subject.GetByCode(SurplusBucket.SurplusCode));
        }

        [TestMethod]
        public void ThreadSafetyCheckOnGetOrAdd()
        {
            InMemoryBudgetBucketRepository subject = Arrange();

            var threads = new List<Thread>();
            int concurrency = 50;
            var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };
            ParallelLoopResult result = Parallel.For(1, concurrency, options, index =>
            {
                var thread = new Thread(ThreadSafetyCheckOneThread);
                threads.Add(thread);
                Console.WriteLine("Starting thread " + index);
                thread.Start(subject);
            });

            while (!result.IsCompleted)
            {
                Thread.Sleep(10);
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
                Console.WriteLine("Thread finished ");
            }

            Assert.IsFalse(this.concurrencyFail);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            if (!isAutoMapperConfigured)
            {
                AutoMapperConfigurationTest.AutoMapperConfiguration();
                isAutoMapperConfigured = true;
            }
        }

        private InMemoryBudgetBucketRepository Arrange()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetBucketDtoTestData());
            return subject;
        }

        private IEnumerable<BudgetBucketDto> CreateBudgetBucketDtoTestData()
        {
            return BudgetModelTestData.CreateBudgetBucketDtoTestData1();
        }

        private BudgetCollection CreateBudgetCollectionModel()
        {
            return BudgetModelTestData.CreateCollectionWith1And2();
        }

        private InMemoryBudgetBucketRepository CreateSubject()
        {
            return new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper());
        }

        private void ThreadSafetyCheckOneThread(object subject)
        {
            var typedSubject = (InMemoryBudgetBucketRepository)subject;
            for (int index = 0; index < 20; index++)
            {
                string code = "AAA";
                try
                {
                    typedSubject.GetOrCreateNew(code, () => new SavedUpForExpenseBucket(code, "GFoo 123"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    this.concurrencyFail = true;
                    throw;
                }
            }
        }
    }
}