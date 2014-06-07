using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
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
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(new BudgetCollection());

            Assert.IsTrue(subject.IsValidCode(JournalBucket.JournalCode));
            Assert.IsInstanceOfType(subject.GetByCode(JournalBucket.JournalCode), typeof(JournalBucket));
        }

        [TestMethod]
        public void AfterInitialiseSurplusBucketShouldExist()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(new BudgetCollection());

            Assert.IsTrue(subject.IsValidCode(SurplusBucket.SurplusCode));
            Assert.IsInstanceOfType(subject.GetByCode(SurplusBucket.SurplusCode), typeof(SurplusBucket));
        }

        [TestMethod]
        public void GetOrAddShouldAddWhenItemDoesntExist()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetCollectionModel());

            subject.GetOrAdd("Foo", () => new IncomeBudgetBucket("Foo", "Bar"));

            Assert.IsTrue(subject.IsValidCode("Foo"));
        }

        [TestMethod]
        public void GetOrAddShouldNotAddWhenItemDoesExist()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetCollectionModel());

            int count = subject.Buckets.Count();
            subject.GetOrAdd(TestDataConstants.HairBucketCode, () =>
            {
                Assert.Fail();
                return new IncomeBudgetBucket(TestDataConstants.HairBucketCode, "Bar");
            });

            Assert.AreEqual(count, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulate6Buckets()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetCollectionModel());

            Assert.AreEqual(6, subject.Buckets.Count());
        }

        [TestMethod]
        public void InitialiseShouldPopulateKnownBuckets()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetCollectionModel());

            Assert.IsTrue(subject.IsValidCode(TestDataConstants.CarMtcBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.HairBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.PowerBucketCode));
            Assert.IsTrue(subject.IsValidCode(TestDataConstants.IncomeBucketCode));
        }

        [TestMethod]
        public void IsValidCodeShouldReturnFalseWhenRepositoryIsEmpty()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            Assert.IsFalse(subject.IsValidCode(SurplusBucket.SurplusCode));
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
            InMemoryBudgetBucketRepository subject = CreateSubject();
            subject.Initialise(CreateBudgetCollectionModel());

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

        [TestMethod]
        public void WhenUnderlyingBudgetCollectionChangesRepositoryReinitialises()
        {
            InMemoryBudgetBucketRepository subject = CreateSubject();
            BudgetCollection collection = CreateBudgetCollectionModel();
            subject.Initialise(collection);

            int count = subject.Buckets.Count();

            var myExpenses = collection.CurrentActiveBudget.Expenses.ToList();
            var myIncomes = collection.CurrentActiveBudget.Incomes.ToList();

            myExpenses.Add(new Expense { Amount = 12.22M, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") });
            collection.CurrentActiveBudget.Update(myIncomes, myExpenses);

            var builder = new StringBuilder();
            collection.Validate(builder);

            Assert.AreEqual(count + 1, subject.Buckets.Count());
        }

        private BudgetCollection CreateBudgetCollectionModel()
        {
            return BudgetModelTestData.CreateCollectionWith1And2();
        }

        private InMemoryBudgetBucketRepository CreateSubject()
        {
            return new InMemoryBudgetBucketRepository();
        }

        private void ThreadSafetyCheckOneThread(object subject)
        {
            var typedSubject = (InMemoryBudgetBucketRepository)subject;
            for (int index = 0; index < 20; index++)
            {
                string code = "AAA";
                try
                {
                    typedSubject.GetOrAdd(code, () => new SavedUpForExpenseBucket(code, "GFoo 123"));
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