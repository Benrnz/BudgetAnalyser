using System;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

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
            return transactionId != null;
        }

        private static void OnNavigateToTransactionCommandExecute([Annotations.NotNull] Guid? transactionId)
        {
            if (transactionId == null)
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