using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class InMemoryBudgetBucketRepositoryTest
{
    private bool concurrencyFail;

    [Fact]
    public void AfterInitialisePayCreditCardBucketShouldExist()
    {
        var subject = CreateSubject().Initialise(new List<BudgetBucketDto>());

        subject.IsValidCode(PayCreditCardBucket.PayCreditCardCode).ShouldBeTrue();
        subject.GetByCode(PayCreditCardBucket.PayCreditCardCode).ShouldBeOfType<PayCreditCardBucket>();
    }

    [Fact]
    public void AfterInitialiseSurplusBucketShouldExist()
    {
        var subject = CreateSubject().Initialise(new List<BudgetBucketDto>());

        subject.IsValidCode(SurplusBucket.SurplusCode).ShouldBeTrue();
        subject.GetByCode(SurplusBucket.SurplusCode).ShouldBeOfType<SurplusBucket>();
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldReturnNewBucket()
    {
        var subject = CreateSubject();
        var result = subject.CreateNewFixedBudgetProject("Foo", "Foo var", 1000);
        result.ShouldNotBeNull();
        subject.IsValidCode(result.Code).ShouldBeTrue();
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenAmountLessThanZero()
    {
        var subject = CreateSubject();
        Should.Throw<ArgumentException>(() => subject.CreateNewFixedBudgetProject("Foo", "Foo bvar", 0));
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenCodeAlreadyExists()
    {
        var subject = CreateSubject();
        subject.GetOrCreateNew(FixedBudgetProjectBucket.CreateCode("Foo"), () => new FixedBudgetProjectBucket("Foo", "Foo bajh", 2000));
        var result = subject.CreateNewFixedBudgetProject("Foo", "Foo var", 1000);
        result.ShouldBeNull();
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenEmptyCode()
    {
        var subject = CreateSubject();
        Should.Throw<ArgumentNullException>(() => subject.CreateNewFixedBudgetProject(string.Empty, "foo bar", 1000));
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenEmptyDescription()
    {
        var subject = CreateSubject();
        Should.Throw<ArgumentNullException>(() => subject.CreateNewFixedBudgetProject("Foo", string.Empty, 1000));
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenNullCode()
    {
        var subject = CreateSubject();
        Should.Throw<ArgumentNullException>(() => subject.CreateNewFixedBudgetProject(null!, "foo bar", 1000));
    }

    [Fact]
    public void CreateNewFixedBudgetProjectShouldThrowGivenNullDescription()
    {
        var subject = CreateSubject();
        Should.Throw<ArgumentNullException>(() => subject.CreateNewFixedBudgetProject("Foo", null!, 1000));
    }

    [Fact]
    public void CtorShouldThrowGivenNullMapper()
    {
        Should.Throw<ArgumentNullException>(() => new InMemoryBudgetBucketRepository(null!));
    }

    [Fact]
    public void GetByCodeShouldThrowGivenNullCode()
    {
        var subject = Arrange();

        Should.Throw<ArgumentNullException>(() => subject.GetByCode(null!));
    }

    [Fact]
    public void GetByOrCreateNewShouldThrowGivenNullCode()
    {
        var subject = Arrange();

        Should.Throw<ArgumentNullException>(() => subject.GetOrCreateNew(null!, () => new PayCreditCardBucket()));
    }

    [Fact]
    public void GetByOrCreateNewShouldThrowGivenNullFactory()
    {
        var subject = Arrange();

        Should.Throw<ArgumentNullException>(() => subject.GetOrCreateNew("CODE", null!));
    }

    [Fact]
    public void GetOrAddShouldAddWhenItemDoesntExist()
    {
        var subject = Arrange();

        subject.GetOrCreateNew("Foo", () => new IncomeBudgetBucket("Foo", "Bar"));

        subject.IsValidCode("Foo").ShouldBeTrue();
    }

    [Fact]
    public void GetOrAddShouldNotAddWhenItemDoesExist()
    {
        var subject = Arrange();

        var count = subject.Buckets.Count();
        var factoryCalled = false;
        subject.GetOrCreateNew(
            TestDataConstants.HairBucketCode,
            () =>
            {
                factoryCalled = true;
                return new IncomeBudgetBucket(TestDataConstants.HairBucketCode, "Bar");
            });

        factoryCalled.ShouldBeFalse();
        subject.Buckets.Count().ShouldBe(count);
    }

    [Fact]
    public void InitialiseShouldPopulate9Buckets()
    {
        var subject = Arrange();
        var expected = CreateBudgetBucketDtoTestData().Count() + 2;
        subject.Buckets.Count().ShouldBe(expected);
    }

    [Fact]
    public void InitialiseShouldPopulateKnownBuckets()
    {
        var subject = Arrange();

        subject.IsValidCode(TestDataConstants.CarMtcBucketCode).ShouldBeTrue();
        subject.IsValidCode(TestDataConstants.HairBucketCode).ShouldBeTrue();
        subject.IsValidCode(TestDataConstants.FoodBucketCode).ShouldBeTrue();
        subject.IsValidCode(TestDataConstants.IncomeBucketCode).ShouldBeTrue();
    }

    [Fact]
    public void InitialiseShouldThrowGivenNullBucketsArgument()
    {
        Should.Throw<ArgumentNullException>(() => CreateSubject().Initialise(null!));
    }

    [Fact]
    public void IsValidCodeShouldReturnFalseWhenRepositoryIsEmpty()
    {
        var subject = CreateSubject();
        subject.IsValidCode(TransactionsListModelTestData.PhoneBucket.Code).ShouldBeFalse();
    }

    [Fact]
    public void IsValidCodeShouldThrowGivenNullCode()
    {
        var subject = Arrange();

        Should.Throw<ArgumentNullException>(() => subject.IsValidCode(null!));
    }

    [Fact]
    public void NewEmptyRepositoryShouldContainSurplusAndPayCC()
    {
        var subject = CreateSubject();
        subject.Buckets.Count().ShouldBe(2);
        subject.Buckets.Any(b => b is SurplusBucket).ShouldBeTrue();
        subject.Buckets.Any(b => b is PayCreditCardBucket).ShouldBeTrue();
    }

    [Fact]
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

        this.concurrencyFail.ShouldBeFalse();
    }

    private InMemoryBudgetBucketRepository Arrange()
    {
        var subject = CreateSubject();
        subject.Initialise(CreateBudgetBucketDtoTestData());
        return subject;
    }

    private static IEnumerable<BudgetBucketDto> CreateBudgetBucketDtoTestData()
    {
        return BudgetModelTestData.CreateBudgetBucketDtoTestData1();
    }

    private static InMemoryBudgetBucketRepository CreateSubject()
    {
        return new InMemoryBudgetBucketRepository(new MapperBudgetBucketToDto2());
    }

    private void ThreadSafetyCheckOneThread(object? subject)
    {
        var typedSubject = (InMemoryBudgetBucketRepository)subject!;
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
