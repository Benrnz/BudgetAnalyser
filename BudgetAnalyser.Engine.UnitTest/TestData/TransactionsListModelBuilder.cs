using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.TestData;

public class TransactionsListModelBuilder
{
    private readonly List<Transaction> additionalTransactions = new();
    private TransactionsListModel model = new(new FakeLogger());

    public TransactionsListModelBuilder AppendTransaction(Transaction transaction)
    {
        this.additionalTransactions.Add(transaction);
        return this;
    }

    public TransactionsListModel Build()
    {
        if (this.additionalTransactions.None())
        {
            return this.model;
        }

        // IEnumerable<MethodInfo> privateMergeMethods = this.model.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(m => m.Name == "Merge");
        // MethodInfo privateMergeMethod = privateMergeMethods.First(m => m.IsPrivate);
        // privateMergeMethod.Invoke(this.model, new object[] { this.additionalTransactions });
        var additionalTransactionsModel = new TransactionsListModel(new FakeLogger()) { LastImport = this.additionalTransactions.Max(t => t.Date).ToDateTime(TimeOnly.MinValue) };
        additionalTransactionsModel.LoadTransactions(this.additionalTransactions);
        return this.model.Merge(additionalTransactionsModel);
    }

    public TransactionsListModelBuilder Merge(TransactionsListModel anotherTransactionsListModel)
    {
        this.model.Merge(anotherTransactionsListModel);
        return this;
    }

    public TransactionsListModelBuilder TestData1()
    {
        this.model = TransactionsListModelTestData.TestData1();
        return this;
    }

    public TransactionsListModelBuilder TestData2()
    {
        this.model = TransactionsListModelTestData.TestData2();
        return this;
    }

    public TransactionsListModelBuilder TestData5()
    {
        this.model = TransactionsListModelTestData.TestData5();
        return this;
    }
}
