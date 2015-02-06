using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class NewBudgetModelController : ControllerBase, IShellDialogInteractivity
    {
        private Guid dialogCorrelationId;
        private IUserMessageBox messageBox;

        public NewBudgetModelController(IUiContext uiContext)
        {
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public event EventHandler Ready;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton
        {
            get { return false; }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton
        {
            get { return EffectiveFrom > DateTime.Today; }
        }

        public DateTime EffectiveFrom { get; set; }

        public void ShowDialog(DateTime defaultEffectiveDate)
        {
            this.dialogCorrelationId = Guid.NewGuid();
            EffectiveFrom = defaultEffectiveDate;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Create new Budget based on current",
                HelpAvailable = true,
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Help)
            {
                this.messageBox.Show("This will clone an exisitng budget, the currently shown budget, to a new budget that is future dated.  The budget must have an effective date in the future.");
                return;
            }

            EventHandler handler = Ready;
            if (handler != null)
            {
                if (message.Response == ShellDialogButton.Cancel)
                {
                }
                else
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }
    }
}