#nullable enable
using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.LedgerBook;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

public class LedgerBookControllerFileOperationsTest
{
    private readonly IApplicationDatabaseFacade applicationDatabaseFacade = Substitute.For<IApplicationDatabaseFacade>();
    private readonly IMessenger messenger = new WeakReferenceMessenger();

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenApplicationDatabaseFacadeIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new LedgerBookControllerFileOperations(this.messenger, null!));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMessengerIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new LedgerBookControllerFileOperations(null!, this.applicationDatabaseFacade));
    }

    [Fact]
    public void Dirty_ShouldNotifyApplicationDatabase_WhenSetToTrue()
    {
        var subject = CreateSubject();

        subject.Dirty = true;

        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.Ledger);
    }

    [Fact]
    public void Dirty_ShouldNotNotifyApplicationDatabase_WhenSetToFalse()
    {
        var subject = CreateSubject();

        subject.Dirty = false;

        this.applicationDatabaseFacade.DidNotReceive().NotifyOfChange(ApplicationDataType.Ledger);
    }

    [Fact]
    public void Close_ShouldClearTheViewModelAndSendClosedMessage()
    {
        var subject = CreateSubject();
        subject.ViewModel.LedgerBook = LedgerBookTestData.TestData1();
        LedgerBookReadyMessage? receivedMessage = null;
        this.messenger.Register<object, LedgerBookReadyMessage>(this, (_, message) => receivedMessage = message);

        subject.Close();

        subject.ViewModel.LedgerBook.ShouldBeNull();
        receivedMessage.ShouldNotBeNull();
        receivedMessage!.LedgerBook.ShouldBeNull();
        receivedMessage.ForceUiRefresh.ShouldBeFalse();
    }

    [Fact]
    public void ReconciliationChangesWillNeedToBeSaved_ShouldMarkLedgerDirtyAndNotifyRelatedDataTypes()
    {
        var subject = CreateSubject();

        subject.ReconciliationChangesWillNeedToBeSaved();

        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.Ledger);
        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.MatchingRules);
        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.Tasks);
    }

    [Fact]
    public void SyncDataFromLedgerService_ShouldDoNothing_WhenLedgerServiceIsNotSet()
    {
        var subject = CreateSubject();
        var ledgerBook = LedgerBookTestData.TestData1();
        subject.ViewModel.LedgerBook = ledgerBook;
        LedgerBookReadyMessage? receivedMessage = null;
        this.messenger.Register<object, LedgerBookReadyMessage>(this, (_, message) => receivedMessage = message);

        subject.SyncDataFromLedgerService();

        subject.ViewModel.LedgerBook.ShouldBeSameAs(ledgerBook);
        receivedMessage.ShouldBeNull();
    }

    [Fact]
    public void SyncDataFromLedgerService_ShouldPopulateTheViewModelAndSendRefreshMessage()
    {
        var subject = CreateSubject();
        var ledgerBook = LedgerBookTestData.TestData2();
        var ledgerService = Substitute.For<ILedgerService>();
        ledgerService.LedgerBook.Returns(ledgerBook);
        subject.LedgerService = ledgerService;
        LedgerBookReadyMessage? receivedMessage = null;
        this.messenger.Register<object, LedgerBookReadyMessage>(this, (_, message) => receivedMessage = message);

        subject.SyncDataFromLedgerService();

        subject.ViewModel.LedgerBook.ShouldBeSameAs(ledgerBook);
        receivedMessage.ShouldNotBeNull();
        receivedMessage!.LedgerBook.ShouldBeSameAs(ledgerBook);
        receivedMessage.ForceUiRefresh.ShouldBeTrue();
    }

    private LedgerBookControllerFileOperations CreateSubject()
    {
        return new LedgerBookControllerFileOperations(this.messenger, this.applicationDatabaseFacade);
    }
}
