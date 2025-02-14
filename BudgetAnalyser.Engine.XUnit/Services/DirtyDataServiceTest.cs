using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class DirtyDataServiceTest
{
    private readonly DirtyDataService service;

    public DirtyDataServiceTest(ITestOutputHelper outputWriter)
    {
        var logger = new XUnitLogger(outputWriter);
        this.service = new DirtyDataService(logger);
    }

    [Fact]
    public void ClearAllDirtyDataFlags_ShouldClearAllFlags()
    {
        this.service.NotifyOfChange(ApplicationDataType.Budget);
        this.service.ClearAllDirtyDataFlags();
        this.service.HasUnsavedChanges.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ShouldThrow_GivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new DirtyDataService(null));
    }

    [Fact]
    public void HasUnsavedChanges_ShouldReturnFalse_WhenNoDataTypeIsDirty()
    {
        this.service.HasUnsavedChanges.ShouldBeFalse();
    }

    [Fact]
    public void HasUnsavedChanges_ShouldReturnTrue_WhenAnyDataTypeIsDirty()
    {
        this.service.NotifyOfChange(ApplicationDataType.Budget);
        this.service.HasUnsavedChanges.ShouldBeTrue();
    }

    [Fact]
    public void IsDirty_ShouldReturnFalse_WhenDataTypeIsNotDirty()
    {
        this.service.IsDirty(ApplicationDataType.Budget).ShouldBeFalse();
    }

    [Fact]
    public void NotifyOfChange_ShouldMarkDataTypeAsDirty()
    {
        this.service.NotifyOfChange(ApplicationDataType.Budget);
        this.service.IsDirty(ApplicationDataType.Budget).ShouldBeTrue();
    }

    [Fact]
    public void SetAllDirtyFlags_ShouldSetAllFlags()
    {
        this.service.SetAllDirtyFlags();
        foreach (ApplicationDataType dataType in Enum.GetValues(typeof(ApplicationDataType)))
        {
            this.service.IsDirty(dataType).ShouldBeTrue();
            this.service.HasUnsavedChanges.ShouldBeTrue();
        }
    }
}
