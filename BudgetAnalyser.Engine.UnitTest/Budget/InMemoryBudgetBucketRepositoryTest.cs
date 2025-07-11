﻿using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Budget;

[TestClass]
public class InMemoryBudgetBucketRepositoryTest
{
    private bool concurrencyFail;

    [TestMethod]
    public void AfterInitialisePayCreditCardBucketShouldExist()
    {
        var subject = CreateSubject().Initialise(new List<BudgetBucketDto>());

        Assert.IsTrue(subject.IsValidCode(PayCreditCardBucket.PayCreditCardCode));
        Assert.IsInstanceOfType(subject.GetByCode(PayCreditCardBucket.PayCreditCardCode), typeof(PayCreditCardBucket));
    }

    [TestMethod]
    public void AfterInitialiseSurplusBucketShouldExist()
    {
        var subject = CreateSubject().Initialise(new List<BudgetBucketDto>());

        Assert.IsTrue(subject.IsValidCode(SurplusBucket.SurplusCode));
        Assert.IsInstanceOfType(subject.GetByCode(SurplusBucket.SurplusCode), typeof(SurplusBucket));
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
    public void CreateNewFixedBudgetProjectShouldThrowGivenCodeAlreadyExists()
    {
        var subject = CreateSubject();
        subject.GetOrCreateNew(FixedBudgetProjectBucket.CreateCode("Foo"), () => new FixedBudgetProjectBucket("Foo", "Foo bajh", 2000));
        var result = subject.CreateNewFixedBudgetProject("Foo", "Foo var", 1000);
        Assert.IsNull(result);
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

        subject.GetOrCreateNew(null, () => new PayCreditCardBucket());

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
        var expected = CreateBudgetBucketDtoTestData().Count() + 2; // Surplus and PayCreditCard are added automatically.
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
        CreateSubject().Initialise(null);

        Assert.Fail();
    }

    [TestMethod]
    public void IsValidCodeShouldReturnFalseWhenRepositoryIsEmpty()
    {
        var subject = CreateSubject();
        Assert.IsFalse(subject.IsValidCode(StatementModelTestData.PhoneBucket.Code));
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
    public void NewEmptyRepositoryShouldContainSurplusAndPayCC()
    {
        var subject = CreateSubject();
        Assert.AreEqual(2, subject.Buckets.Count());
        Assert.IsTrue(subject.Buckets.Any(b => b is SurplusBucket));
        Assert.IsTrue(subject.Buckets.Any(b => b is PayCreditCardBucket));
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

    private InMemoryBudgetBucketRepository CreateSubject()
    {
        return new InMemoryBudgetBucketRepository(new MapperBudgetBucketToDto2());
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
}
