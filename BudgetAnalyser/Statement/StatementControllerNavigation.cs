using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class StatementControllerNavigation
    {
        private readonly StatementController controller;
        private readonly IUserQuestionBoxYesNo questionBox;

        public StatementControllerNavigation(
            [NotNull] IMessenger messenger,
            [NotNull] StatementController controller,
            [NotNull] IUserQuestionBoxYesNo questionBox)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (questionBox == null)
            {
                throw new ArgumentNullException(nameof(questionBox));
            }

            MessengerInstance = messenger;
            this.controller = controller;
            this.questionBox = questionBox;

            MessengerInstance.Register<StatementControllerNavigation, NavigateToTransactionMessage>(this, static (r, m) => r.OnNavigateToTransactionRequestReceived(m));
        }

        private IMessenger MessengerInstance { get; }
        private StatementViewModel ViewModel => this.controller.ViewModel;

        private bool NavigateToTransactionOutsideOfFilter(Guid transactionId)
        {
            Transaction foundTransaction = ViewModel.Statement.AllTransactions.FirstOrDefault(t => t.Id == transactionId);
            if (foundTransaction is not null)
            {
                bool? result = this.questionBox.Show("The transaction falls outside the current filter. Do you wish to adjust the filter to show the transaction?", "Navigate to Transaction");
                if (result == null || !result.Value)
                {
                    return false;
                }

                GlobalFilterCriteria newCriteria;
                var requestCurrentFilter = new RequestFilterMessage(this);
                MessengerInstance.Send(requestCurrentFilter);

                if (foundTransaction.Date < requestCurrentFilter.Criteria.BeginDate)
                {
                    newCriteria = new GlobalFilterCriteria { BeginDate = foundTransaction.Date, EndDate = requestCurrentFilter.Criteria.EndDate };
                }
                else
                {
                    newCriteria = new GlobalFilterCriteria { BeginDate = requestCurrentFilter.Criteria.BeginDate, EndDate = foundTransaction.Date };
                }

                MessengerInstance.Send(new RequestFilterChangeMessage(this) { Criteria = newCriteria });

                return NavigateToVisibleTransaction(transactionId);
            }

            return false;
        }

        private bool NavigateToVisibleTransaction(Guid transactionId)
        {
            Transaction foundTransaction = ViewModel.Statement.Transactions.FirstOrDefault(t => t.Id == transactionId);
            if (foundTransaction is not null)
            {
                ViewModel.SelectedRow = foundTransaction;
                return true;
            }

            return false;
        }

        private void OnNavigateToTransactionRequestReceived(NavigateToTransactionMessage message)
        {
            if (NavigateToVisibleTransaction(message.TransactionId))
            {
                message.SetSearchAsSuccessful();
                return;
            }

            if (NavigateToTransactionOutsideOfFilter(message.TransactionId))
            {
                message.SetSearchAsSuccessful();
                return;
            }

            message.SetSearchAsFailed();
            // No such transaction id found.
        }
    }
}