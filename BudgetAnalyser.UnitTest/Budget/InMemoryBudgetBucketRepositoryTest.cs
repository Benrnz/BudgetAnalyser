using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class InMemoryBudgetBucketRepositoryTest
    {
        private bool concurrencyFail;

        [TestMethod]
        public void AfterInitialiseJournalBucketShouldExist()
        {
            var subject = CreateSubject();
            subject.Initialise(new List<BudgetBucketDto>());

            Assert.IsTrue(subject.IsValidCode(JournalBucket.JournalCode));
            Assert.IsInstanceOfType(subject.GetByCode(JournalBucket.JournalCode), typeof(JournalBucket));
        }

        [TestMethod]
        public void AfterInitialiseSurplusBucketShouldExist()
        {
            var subject = CreateSubject();
            subject.Initialise(new List<BudgetBucketDto>());

            Assert.IsTrue(subject.IsValidCode(SurplusBucket.SurplusCode));
            Assert.IsInstanceOfType(subject.GetByCode(SurplusBucket.SurplusCode), typeof(SurplusBucket));
        }

        private InMemoryBudgetBucketRepository Arrange()
        {
            var subject = CreateSubject();
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

        [TestMethod]
        public void CreateNewFixedBudgetProjectShouldReturnNewBucket()
        {
            var subject = CreateSubject();
            var result = subject.CreateNewFixedBudgetProject("Foo", "Foo var", 1000);
            Assert.IsNotNull(result);
            Assert.IsTrue(subject.IsValidCode(result.Code));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenAmountLessThanZero()
        {
            var subject = CreateSubject();
            subject.CreateNewFixedBudgetProject("Foo", "Foo bvar", 0);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenCodeAlreadyExists()
        {
            var subject = CreateSubject();
            subject.GetOrCreateNew(FixedBudgetProjectBucket.CreateCode("Foo"), () => new FixedBudgetProjectBucket("Foo", "Foo bajh", 2000));
            subject.CreateNewFixedBudgetProject("Foo", "Foo var", 1000);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenEmptyCode()
        {
            var subject = CreateSubject();
            subject.CreateNewFixedBudgetProject(string.Empty, "foo bar", 1000);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenEmptyDescription()
        {
            var subject = CreateSubject();
            subject.CreateNewFixedBudgetProject("Foo", string.Empty, 1000);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenNullCode()
        {
            var subject = CreateSubject();
            subject.CreateNewFixedBudgetProject(null, "foo bar", 1000);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewFixedBudgetProjectShouldThrowGivenNullDescription()
        {
            var subject = CreateSubject();
            subject.CreateNewFixedBudgetProject("Foo", null, 1000);
            Assert.Fail();
        }

        private InMemoryBudgetBucketRepository CreateSubject()
        {
            return new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper());
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
            var subject = Arrange();

            subject.GetByCode(null);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetByOrCreateNewShouldThrowGivenNullCode()
        {
            var subject = Arrange();

            subject.GetOrCreateNew(null, () => new JournalBucket());

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetByOrCreateNewShouldThrowGivenNullFactory()
        {
            var subject = Arrange();

            subject.GetOrCreateNew("CODE", null);

            Assert.Fail();
        }

        [TestMethod]
        public void GetOrAddShouldAddWhenItemDoesntExist()
        {
            var subject = Arrange();

            subject.GetOrCreateNew("Foo", () => new IncomeBudgetBucket("Foo", "Bar"));

            Assert.IsTrue(subject.IsValidCode("Foo"));
        }

        [TestMethod]
        public void GetOrAddShouldNotAddWhenItemDoesExist()
        {
            var subject = Arrange();

            var count = subject.Buckets.Count();
            subject.GetOrCreateNew(
                TestDataConstants.HairBucketCode,
                () =>
                {
                    Assert.Fail();
                    return new IncomeBudgetBucket(TestDataConstants.HairBucketCode, "Bar");
                });

            Assert.AreEqual(count, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulate9Buckets()
        {
            var subject = Arrange();
            var expected = CreateBudgetBucketDtoTestData().Count() + 2; // Surplus and Journal are added automatically.
            Assert.AreEqual(expected, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulateKnownBuckets()
        {
            var subject = Arrange();

            Assert.IsTrue(subject.IsValidCode(TestDataConstants.CarMtcBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.HairBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.FoodBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.IncomeBucketCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitialiseShouldThrowGivenNullBucketsArgument()
        {
            var subject = CreateSubject();
            subject.Initialise(null);

            Assert.Fail();
        }

        [TestMethod]
        public void IsValidCodeShouldReturnFalseWhenRepositoryIsEmpty()
        {
            var subject = CreateSubject();
            Assert.IsFalse(subject.IsValidCode(SurplusBucket.SurplusCode));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsValidCodeShouldThrowGivenNullCode()
        {
            var subject = Arrange();

            subject.IsValidCode(null);

            Assert.Fail();
        }

        [TestMethod]
        public void NewRepositoryShouldBeEmpty()
        {
            var subject = CreateSubject();
            Assert.IsFalse(subject.Buckets.Any());
        }

        [TestMethod]
        public void NewRepositoryShouldNotContainDefaultBuckets()
        {
            var subject = CreateSubject();
            Assert.IsNull(subject.GetByCode(SurplusBucket.SurplusCode));
        }

        private void ThreadSafetyCheckOneThread(object subject)
        {
            var typedSubject = (InMemoryBudgetBucketRepository)subject;
            for (var index = 0; index < 20; index++)
            {
                var code = "AAA";
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

        [TestMethod]
        public void ThreadSafetyCheckOnGetOrAdd()
        {
            var subject = Arrange();

            var threads = new List<Thread>();
            var concurrency = 50;
            var options = new ParallelOptions { MaxDegreeOfParallelism = 20 };
            var result = Parallel.For(
                1,
                concurrency,
                options,
                index =>
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

            foreach (var thread in threads)
            {
                thread.Join();
                Console.WriteLine("Thread finished ");
            }

            Assert.IsFalse(this.concurrencyFail);
        }
    }
}