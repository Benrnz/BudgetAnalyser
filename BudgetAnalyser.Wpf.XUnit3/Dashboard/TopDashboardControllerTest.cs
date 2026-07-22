using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Mobile;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Dashboard;

public class TopDashboardControllerTest
{
    [Fact]
    public async Task ApplicationStateLoadedMessage_ShouldLoadPersistedDatabaseKey()
    {
        var context = CreateContext();
        var loadSignal = new TaskCompletionSource<bool>();
        context.ApplicationDatabaseFacade.LoadAsync("D:\\Temp\\rehydrated.bax").Returns(Task.FromResult(new ApplicationDatabase()));
        context.ApplicationDatabaseFacade.When(x => x.LoadAsync("D:\\Temp\\rehydrated.bax")).Do(_ => loadSignal.SetResult(true));

        var message = new ApplicationStateLoadedMessage(
        [
            new ApplicationEngineState
            {
                BudgetAnalyserDataStorageKey = "D:\\Temp\\rehydrated.bax"
            }
        ]);

        context.Messenger.Send(message);

        await WaitForSignal(loadSignal.Task);
        context.ApplicationDatabaseFacade.Received(1).Close();
        await context.ApplicationDatabaseFacade.Received(1).LoadAsync("D:\\Temp\\rehydrated.bax");
    }

    [Fact]
    public void ApplicationStateRequestedMessage_ShouldPersistPreparedEngineState()
    {
        var context = CreateContext();
        var expectedState = new ApplicationEngineState { BudgetAnalyserDataStorageKey = "D:\\Temp\\test-file.bax" };
        context.ApplicationDatabaseFacade.PreparePersistentStateData().Returns(expectedState);
        var message = new ApplicationStateRequestedMessage();

        context.Messenger.Send(message);

        var persistedState = message.PersistentData.OfType<ApplicationEngineState>().Single();
        persistedState.ShouldNotBeNull();
        persistedState.BudgetAnalyserDataStorageKey.ShouldBe(expectedState.BudgetAnalyserDataStorageKey);
    }

    [Fact]
    public void WidgetActivatedMessage_WithDaysSinceLastImport_ShouldRequestTransactionsTab()
    {
        var context = CreateContext();
        MainMenuTabRequestMessage observed = null!;
        var observer = new object();
        context.Messenger.Register<object, MainMenuTabRequestMessage>(observer, (_, message) => observed = message);

        context.Messenger.Send(new WidgetActivatedMessage(new DaysSinceLastImport()));

        observed.ShouldNotBeNull();
        observed.Tab.ShouldBe(MainMenuTab.Transactions);
    }

    [Fact]
    public async Task WidgetActivatedMessage_WithSaveWidget_ShouldSaveDatabase()
    {
        var context = CreateContext();
        var saveSignal = new TaskCompletionSource<bool>();
        context.ApplicationDatabaseFacade.HasUnsavedChanges.Returns(true);
        context.ApplicationDatabaseFacade.ValidateAll(Arg.Any<StringBuilder>()).Returns(true);
        context.ApplicationDatabaseFacade.SaveAsync().Returns(Task.CompletedTask);
        context.ApplicationDatabaseFacade.When(x => x.SaveAsync()).Do(_ => saveSignal.SetResult(true));
        PrivateAccessor.SetField(context.PersistenceOperations, "lastSave", DateTime.Now.AddMinutes(-5));

        context.Messenger.Send(new WidgetActivatedMessage(new SaveWidget()));

        await WaitForSignal(saveSignal.Task);
        await context.ApplicationDatabaseFacade.Received(1).SaveAsync();
    }

    private static TestContext CreateContext()
    {
        var messenger = new WeakReferenceMessenger();
        var logger = Substitute.For<ILogger>();
        var messageBox = Substitute.For<IUserMessageBox>();
        var yesNoBox = Substitute.For<IUserQuestionBoxYesNo>();
        var inputBox = Substitute.For<IUserInputBox>();
        var applicationDatabaseFacade = Substitute.For<IApplicationDatabaseFacade>();
        var applicationDatabaseService = Substitute.For<IApplicationDatabaseService>();
        var ruleService = Substitute.For<ITransactionRuleService>();
        var dashboardService = Substitute.For<IDashboardService>();
        var mobileExporter = Substitute.For<IMobileDataExporter>();
        var mobileUploader = Substitute.For<IMobileDataUploader>();

        var userPrompts = new UserPrompts(
            messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            yesNoBox,
            inputBox);

        applicationDatabaseService.GlobalFilter.Returns(new GlobalFilterCriteria());
        applicationDatabaseFacade.PreparePersistentStateData().Returns(new ApplicationEngineState { BudgetAnalyserDataStorageKey = "D:\\Temp\\default.bax" });
        applicationDatabaseFacade.LoadAsync(Arg.Any<string>()).Returns(Task.FromResult(new ApplicationDatabase()));
        applicationDatabaseFacade.SaveAsync().Returns(Task.CompletedTask);
        applicationDatabaseFacade.ValidateAll(Arg.Any<StringBuilder>()).Returns(true);
        dashboardService.WidgetsToDisplay().Returns(new ObservableCollection<WidgetGroup>());

        var disusedRulesController = new DisusedRulesController(messenger, ruleService, applicationDatabaseFacade);
        var globalFilterController = new GlobalFilterController(messenger, userPrompts, applicationDatabaseService);
        var uploadMobileDataController = new UploadMobileDataController(messenger, logger, userPrompts, mobileExporter, mobileUploader, applicationDatabaseFacade);
        var encryptFileController = new EncryptFileController(messenger, userPrompts, applicationDatabaseFacade);
        var persistenceOperations = new PersistenceOperations(messenger, logger, userPrompts, applicationDatabaseFacade, new DemoFileHelper(), encryptFileController);
        var subject = new TopDashboardController(
            messenger,
            logger,
            userPrompts,
            disusedRulesController,
            globalFilterController,
            uploadMobileDataController,
            dashboardService,
            persistenceOperations);

        return new TestContext(subject, messenger, applicationDatabaseFacade, persistenceOperations);
    }

    private static async Task WaitForSignal(Task signal)
    {
        var completed = await Task.WhenAny(signal, Task.Delay(3000));
        completed.ShouldBe(signal);
        await signal;
    }

    private sealed record TestContext(
        TopDashboardController Subject,
        IMessenger Messenger,
        IApplicationDatabaseFacade ApplicationDatabaseFacade,
        PersistenceOperations PersistenceOperations);
}
