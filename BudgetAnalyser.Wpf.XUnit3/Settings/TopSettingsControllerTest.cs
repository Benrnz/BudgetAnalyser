using System.ComponentModel;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Settings;
using BudgetAnalyser.Transactions;
using BudgetAnalyser.Wpf.XUnit3.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Settings;

public class TopSettingsControllerTest
{
    [Fact]
    public void ApplicationStateLoadedMessage_ShouldLeaveLastBaxFileEmpty_WhenNoEngineStateWasStored()
    {
        var (subject, messenger, _) = CreateSubject();

        messenger.Send(new ApplicationStateLoadedMessage([new ShellPersistentState { ListPageSize = 25 }]));

        subject.LastBaxFile.ShouldBeEmpty();
    }

    [Fact]
    public void ApplicationStateLoadedMessage_ShouldPopulateLastBaxFile()
    {
        var (subject, messenger, _) = CreateSubject();

        messenger.Send(new ApplicationStateLoadedMessage([new ApplicationEngineState { BudgetAnalyserDataStorageKey = "D:\\Temp\\rehydrated.bax" }]));

        subject.LastBaxFile.ShouldBe("D:\\Temp\\rehydrated.bax");
    }

    [Fact]
    public void ListPageSize_ShouldReadFromTransactionsController()
    {
        var (subject, _, transactions) = CreateSubject();

        transactions.PageSize = 42;

        subject.ListPageSize.ShouldBe(42);
    }

    [Fact]
    public void ListPageSize_ShouldUpdateTransactionsController_WhenSet()
    {
        var (subject, _, transactions) = CreateSubject();

        subject.ListPageSize = 50;

        transactions.PageSize.ShouldBe(50);
    }

    [Fact]
    public void Shown_ShouldRaiseListPageSizeChanged_WhenShown()
    {
        var (subject, _, _) = CreateSubject();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)subject).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        subject.Shown = true;

        changed.ShouldContain(nameof(TopSettingsController.ListPageSize));
    }

    private static (TopSettingsController Subject, IMessenger Messenger, TopTransactionsListController Transactions) CreateSubject()
    {
        var context = TopTransactionsListControllerTest.CreateContext();
        var subject = new TopSettingsController(context.Messenger, context.Subject);
        return (subject, context.Messenger, context.Subject);
    }
}
