using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public static class LedgerTransactionCommands
    {
        [PropertyInjection]
        public static IMessenger MessengerInstance { get; [UsedImplicitly] set; }

        public static ICommand NavigateToTransactionCommand => new RelayCommand<Guid?>(OnNavigateToTransactionCommandExecute, CanExecuteNavigateToTransactionCommand);

        private static bool CanExecuteNavigateToTransactionCommand(Guid? transactionId)
        {
            return transactionId is not null;
        }

        private static void OnNavigateToTransactionCommandExecute([NotNull] Guid? transactionId)
        {
            if (transactionId is null)
            {
                throw new ArgumentNullException(nameof(transactionId));
            }

            using (var message = new NavigateToTransactionMessage(transactionId.Value))
            {
                MessengerInstance.Send(message);
            }
        }
    }
}
