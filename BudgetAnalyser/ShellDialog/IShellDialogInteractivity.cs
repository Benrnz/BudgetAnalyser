namespace BudgetAnalyser.ShellDialog
{
    /// <summary>
    ///     An optional interface that can be implented by any view model given to the
    ///     <see cref="ShellDialogRequestMessage.Content" />.
    ///     If implemented it can add additional interactivity to the Shel Dialog.
    /// </summary>
    public interface IShellDialogInteractivity
    {
        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        bool CanExecuteCancelButton { get; }

        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        bool CanExecuteOkButton { get; }

        /// <summary>
        ///     Will be called ascertain the availability of the button.
        /// </summary>
        bool CanExecuteSaveButton { get; }
    }
}