using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ShellDialog;

public class ShellDialogController : ControllerBase
{
    private readonly RelayCommand<ShellDialogButton> dialogRelayCommand;

    public ShellDialogController(IMessenger messenger) : base(messenger)
    {
        DialogType = ShellDialogType.OkCancel;
        this.dialogRelayCommand = new RelayCommand<ShellDialogButton>(OnDialogCommandExecute, CanExecuteDialogCommand);
        DialogCommand = this.dialogRelayCommand;
        messenger.Register<ShellDialogCommandRequerySuggestedMessage>(this, (_, _) => this.dialogRelayCommand.NotifyCanExecuteChanged());
    }

    public string ActionToolTip => DialogType == ShellDialogType.SaveCancel ? "Save" : "Ok";

    public bool CancelButtonVisible
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string CloseToolTip => DialogType == ShellDialogType.Close ? "Close" : "Cancel";

    public object? Content
    {
        get;
        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            this.dialogRelayCommand.NotifyCanExecuteChanged();
        }
    }

    public Guid CorrelationId { get; set; }

    public ICommand DialogCommand { get; private init; }

    public ShellDialogType DialogType
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OkButtonVisible = DialogType == ShellDialogType.Ok || DialogType == ShellDialogType.OkCancel;
            SaveButtonVisible = DialogType == ShellDialogType.SaveCancel;
            CancelButtonVisible = DialogType != ShellDialogType.Ok;
            OnPropertyChanged(nameof(ActionToolTip));
            OnPropertyChanged(nameof(CloseToolTip));
            OnPropertyChanged();
        }
    }

    public bool HelpButtonVisible
    {
        [UsedImplicitly]
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public bool OkButtonVisible
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OkIsCancel));
        }
    }

    public bool OkIsCancel => OkButtonVisible && !CancelButtonVisible && !SaveButtonVisible;

    public bool SaveButtonVisible
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public string Title
    {
        [UsedImplicitly]
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     Intended to be called from the <see cref="ShellController" /> so it can orchestrate showing the dialog.
    ///     Each mega-tab of the application has its own instance of this <see cref="ShellDialogController" />.
    /// </summary>
    /// <param name="message">The message containing all the parameters to configure and show the dialog.</param>
    public void ShowFromShell(ShellDialogRequestMessage message)
    {
        Title = message.Title;
        Content = message.Content;
        DialogType = message.DialogType;
        CorrelationId = message.CorrelationId;
        HelpButtonVisible = message.HelpAvailable;
    }

    private bool CanExecuteDialogCommand(ShellDialogButton arg)
    {
        if (Content is null)
        {
            return false;
        }

        if (Content is not IShellDialogInteractivity dialogInteractivity)
        {
            return true;
        }

        switch (arg)
        {
            case ShellDialogButton.Cancel:
                return dialogInteractivity.CanExecuteCancelButton;
            case ShellDialogButton.Ok:
                return dialogInteractivity.CanExecuteOkButton;
            case ShellDialogButton.Save:
                return dialogInteractivity.CanExecuteSaveButton;
        }

        return true;
    }

    private void OnDialogCommandExecute(ShellDialogButton commandType)
    {
        // Delay execution so that keyed events happen
        Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            () =>
            {
                if (Content is null)
                {
                    return;
                }

                // No correlation id given so no response is expected.
                if (CorrelationId != Guid.Empty)
                {
                    switch (commandType)
                    {
                        case ShellDialogButton.Ok:
                        case ShellDialogButton.Save:
                            if (!OkButtonVisible && commandType == ShellDialogButton.Ok)
                            {
                                // If both Ok and Save are not visible, then assume Cancel behaviour
                                commandType = SaveButtonVisible ? ShellDialogButton.Save : ShellDialogButton.Cancel;
                            }

                            // Use the visible action as the default response when Enter is pressed.
                            Messenger.Send(new ShellDialogResponseMessage(Content, commandType) { CorrelationId = CorrelationId });
                            break;

                        case ShellDialogButton.Cancel:
                            Messenger.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Cancel) { CorrelationId = CorrelationId });
                            break;

                        case ShellDialogButton.Help:
                            Messenger.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Help) { CorrelationId = CorrelationId });
                            // Don't close the dialog after this button click is processed.
                            return;

                        default:
                            throw new NotSupportedException("Unsupported command type received from Shell Dialog on Shell view. " + commandType);
                    }
                }

                // Setting the content to null will hide the dialog, its visibility is bound to the Content is not null
                Content = null;
            });
    }
}
