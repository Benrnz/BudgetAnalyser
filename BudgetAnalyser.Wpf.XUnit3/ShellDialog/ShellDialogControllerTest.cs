using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.ShellDialog;

public class ShellDialogControllerTest
{
    private readonly IMessenger messenger = Substitute.For<IMessenger>();

    [Fact]
    public void DialogType_Close_ShouldOnlyShowCancelButton()
    {
        var subject = new ShellDialogController(this.messenger);

        subject.DialogType = ShellDialogType.Close;

        subject.OkButtonVisible.ShouldBeFalse();
        subject.SaveButtonVisible.ShouldBeFalse();
        subject.CancelButtonVisible.ShouldBeTrue();
    }
}
