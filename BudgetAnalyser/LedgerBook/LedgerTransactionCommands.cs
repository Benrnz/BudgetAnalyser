using System;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public static class LedgerTransactionCommands
    {
        [PropertyInjection]
        public static IMessenger MessengerInstance { get; set; }

        public static ICommand NavigateToTransactionCommand
        {
            get { return new RelayCommand<Guid?>(OnNavigateToTransactionCommandExecute, CanExecuteNavigateToTransactionCommand); }
        }

        private static bool CanExecuteNavigateToTransactionCommand(Guid? transactionId)
        {
            return transactionId != null;
        }

        private static void OnNavigateToTransactionCommandExecute([NotNull] Guid? transactionId)
        {
            if (transactionId == null)
            {
                throw new ArgumentNullException("transactionId");
            }

            using (var message = new NavigateToTransactionMessage(transactionId.Value))
            {
                MessengerInstance.Send(message);
            }
        }
    }
}