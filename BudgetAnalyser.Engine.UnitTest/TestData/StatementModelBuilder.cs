using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.TestData;

public class StatementModelBuilder
{
    private readonly List<Transaction> additionalTransactions = new();
    private TransactionSetModel model = new(new FakeLogger());

    public StatementModelBuilder AppendTransaction(Transaction transaction)
    {
        this.additionalTransactions.Add(transaction);
        return this;
    }

    public TransactionSetModel Build()
    {
        if (this.additionalTransactions.None())
        {
            return this.model;
        }

        // IEnumerable<MethodInfo> privateMergeMethods = this.model.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(m => m.Name == "Merge");
        // MethodInfo privateMergeMethod = privateMergeMethods.First(m => m.IsPrivate);
        // privateMergeMethod.Invoke(this.model, new object[] { this.additionalTransactions });
        var additionalTransactionsModel = new TransactionSetModel(new FakeLogger()) { LastImport = this.additionalTransactions.Max(t => t.Date).ToDateTime(TimeOnly.MinValue) };
        additionalTransactionsModel.LoadTransactions(this.additionalTransactions);
        return this.model.Merge(additionalTransactionsModel);
    }

    public StatementModelBuilder Merge(TransactionSetModel anotherTransactionSetModel)
    {
        this.model.Merge(anotherTransactionSetModel);
        return this;
    }

    public StatementModelBuilder TestData1()
    {
        this.model = StatementModelTestData.TestData1();
        return this;
    }

    public StatementModelBuilder TestData2()
    {
        this.model = StatementModelTestData.TestData2();
        return this;
    }

    public StatementModelBuilder TestData5()
    {
        this.model = StatementModelTestData.TestData5();
        return this;
    }
}
