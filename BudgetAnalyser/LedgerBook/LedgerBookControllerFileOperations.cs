using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public class LedgerBookControllerFileOperations(IMessenger messenger, IApplicationDatabaseFacade applicationDatabaseService) : INotifyPropertyChanged
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));

    public event PropertyChangedEventHandler? PropertyChanged;

    internal bool Dirty
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            if (value)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Ledger);
            }
        }
    }

    /// <summary>
    ///     Gets or sets the ledger service. Will be set by the <see cref="TopLedgerBookController" /> during its initialisation.
    /// </summary>
    internal ILedgerService? LedgerService { get; set; }

    private IMessenger MessengerInstance { get; } = messenger ?? throw new ArgumentNullException(nameof(messenger));

    public LedgerBookViewModel ViewModel { get; } = new();

    public void Close()
    {
        ViewModel.LedgerBook = null;
        MessengerInstance.Send(new LedgerBookReadyMessage(null));
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void ReconciliationChangesWillNeedToBeSaved()
    {
        Dirty = true;
        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Tasks);
    }

    internal void SyncDataFromLedgerService()
    {
        if (LedgerService is null)
        {
            return;
        }

        ViewModel.LedgerBook = LedgerService.LedgerBook;
        MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
    }
}
