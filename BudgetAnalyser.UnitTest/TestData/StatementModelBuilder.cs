using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    public class StatementModelBuilder
    {
        private StatementModel model = new StatementModel(new FakeLogger());
        private readonly List<Transaction> additionalTransactions = new List<Transaction>();

        public StatementModelBuilder TestData1()
        {
            this.model = StatementModelTestData.TestData1();
            return this;
        }

        public StatementModel Build()
        {
            if (this.additionalTransactions.None())
            {
                return this.model;
            }

            var privateMergeMethods = this.model.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(m => m.Name == "Merge");
            var privateMergeMethod = privateMergeMethods.First(m => m.IsPrivate);
            privateMergeMethod.Invoke(this.model, new object[] { this.additionalTransactions });
            return this.model;
        }

        public StatementModelBuilder AppendTransaction(Transaction transaction)
        {
            this.additionalTransactions.Add(transaction);
            return this;
        }

        public StatementModelBuilder Merge(StatementModel anotherStatementModel)
        {
            this.model.Merge(anotherStatementModel);
            return this;
        }
    }
}
