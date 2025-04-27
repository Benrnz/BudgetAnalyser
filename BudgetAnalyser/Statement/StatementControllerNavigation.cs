using BudgetAnalyser.Engine;
using BudgetAnalyser.Filtering;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class StatementControllerNavigation
{
    private readonly StatementController controller;
    private readonly IUserQuestionBoxYesNo questionBox;

    public StatementControllerNavigation(IMessenger messenger, StatementController controller, IUserQuestionBoxYesNo questionBox)
    {
        MessengerInstance = messenger ?? throw new ArgumentNullException(nameof(messenger));
        this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        this.questionBox = questionBox ?? throw new ArgumentNullException(nameof(questionBox));
    }

    private IMessenger MessengerInstance { get; }
    private StatementViewModel ViewModel => this.controller.ViewModel;

    private bool NavigateToTransactionOutsideOfFilter(Guid transactionId)
    {
        var foundTransaction = ViewModel.Statement.AllTransactions.FirstOrDefault(t => t.Id == transactionId);
        if (foundTransaction is not null)
        {
            var result = this.questionBox.Show("The transaction falls outside the current filter. Do you wish to adjust the filter to show the transaction?", "Navigate to Transaction");
            if (result is null || !result.Value)
            {
                return false;
            }

            GlobalFilterCriteria newCriteria;
            var requestCurrentFilter = new RequestFilterMessage(this);
            MessengerInstance.Send(requestCurrentFilter);

            newCriteria = foundTransaction.Date < requestCurrentFilter.Criteria.BeginDate
                ? new GlobalFilterCriteria { BeginDate = foundTransaction.Date, EndDate = requestCurrentFilter.Criteria.EndDate }
                : new GlobalFilterCriteria { BeginDate = requestCurrentFilter.Criteria.BeginDate, EndDate = foundTransaction.Date };

            MessengerInstance.Send(new RequestFilterChangeMessage(this) { Criteria = newCriteria });

            return NavigateToVisibleTransaction(transactionId);
        }

        return false;
    }

    private bool NavigateToVisibleTransaction(Guid transactionId)
    {
        var foundTransaction = ViewModel.Statement.Transactions.FirstOrDefault(t => t.Id == transactionId);
        if (foundTransaction is not null)
        {
            ViewModel.SelectedRow = foundTransaction;
            return true;
        }

        return false;
    }
}
