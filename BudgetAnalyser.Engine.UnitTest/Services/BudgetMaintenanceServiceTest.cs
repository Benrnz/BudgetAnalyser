using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services;

[TestClass]
public class BudgetMaintenanceServiceTest
{
    private Mock<IBudgetBucketRepository> bucketRepo;
    private BudgetCollection budgetCollection;
    private Mock<IBudgetRepository> budgetRepo;
    private Mock<ILogger> logger;
    private FakeMonitorableDependencies monitorableDependencies;

    private BudgetMaintenanceService service;

    [TestMethod]
    public void CloneBudgetModel_ShouldAddToCollection()
    {
        var source = this.budgetCollection.CurrentActiveBudget;

        Assert.IsTrue(this.budgetCollection.Count == 1);
        var result = this.service.CloneBudgetModel(source, DateTime.Today.AddDays(1), BudgetCycle.Monthly);
        Assert.IsTrue(this.budgetCollection.Count == 2);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CloneBudgetModel_WhenEffectiveDateIsPast_ShouldFail()
    {
        var source = this.budgetCollection.CurrentActiveBudget;

        this.service.CloneBudgetModel(source, DateTime.Today, BudgetCycle.Monthly);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CloneBudgetModel_WhenNewBudgetEffectiveFromIsBeforeOrEqualSourceBudgetEffectiveFrom_ThrowsArgumentException()
    {
        // Arrange
        var sourceBudget = new BudgetModel
        {
            EffectiveFrom = DateTime.Today
        };
        var newBudgetEffectiveFrom = DateTime.Today;
        var budgetCycle = BudgetCycle.Monthly;

        // Act + Assert
        this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CloneBudgetModel_WhenNewBudgetEffectiveFromIsLessThanOrEqualToToday_ThrowsArgumentException()
    {
        // Arrange
        var sourceBudget = new BudgetModel
        {
            EffectiveFrom = DateTime.Today
        };
        var newBudgetEffectiveFrom = DateTime.Today.AddDays(-1);
        var budgetCycle = BudgetCycle.Monthly;

        // Act + Assert
        this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CloneBudgetModel_WhenSourceBudgetIsEmpty_ShouldFail()
    {
        var source = new BudgetModel();

        this.service.CloneBudgetModel(source, DateTime.Today, BudgetCycle.Monthly);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CloneBudgetModel_WhenSourceBudgetIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        BudgetModel sourceBudget = null;
        var newBudgetEffectiveFrom = DateTime.Today.AddDays(1);
        var budgetCycle = BudgetCycle.Monthly;

        // Act + Assert
        this.service.CloneBudgetModel(sourceBudget, newBudgetEffectiveFrom, budgetCycle);
    }

    [TestMethod]
    public void CloneBudgetModel_WhenSourceCycleIsMonthly_ShouldBeAbleToSetClonedToFortnightly()
    {
        var source = this.budgetCollection.CurrentActiveBudget;

        source.BudgetCycle = BudgetCycle.Monthly;
        var result = this.service.CloneBudgetModel(source, DateTime.Today.AddDays(1), BudgetCycle.Fortnightly);
        Assert.AreEqual(BudgetCycle.Fortnightly, result.BudgetCycle);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WhenBudgetBucketRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act + Assert
        new BudgetMaintenanceService(this.budgetRepo.Object, null, this.logger.Object, this.monitorableDependencies);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WhenBudgetRepositoryIsNull_ThrowsArgumentNullException()
    {
        // Act + Assert
        new BudgetMaintenanceService(null, this.bucketRepo.Object, this.logger.Object, this.monitorableDependencies);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act + Assert
        new BudgetMaintenanceService(this.budgetRepo.Object, this.bucketRepo.Object, null, this.monitorableDependencies);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WhenMonitorableDependenciesIsNull_ThrowsArgumentNullException()
    {
        // Act + Assert
        new BudgetMaintenanceService(this.budgetRepo.Object, this.bucketRepo.Object, this.logger.Object, null);
    }

    [TestInitialize]
    public void SetUp()
    {
        this.budgetRepo = new Mock<IBudgetRepository>();
        this.bucketRepo = new Mock<IBudgetBucketRepository>();
        this.logger = new Mock<ILogger>();
        this.monitorableDependencies = new FakeMonitorableDependencies();
        this.budgetCollection = new BudgetCollection(BudgetModelTestData.CreateTestData1());

        this.service = new BudgetMaintenanceService(
                                                    this.budgetRepo.Object,
                                                    this.bucketRepo.Object,
                                                    this.logger.Object,
                                                    this.monitorableDependencies);

        PrivateAccessor.SetProperty(this.service, "Budgets", this.budgetCollection);
    }
}