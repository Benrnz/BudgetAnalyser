using System;
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
        private readonly List<Transaction> additionalTransactions = new List<Transaction>();
        private StatementModel model = new StatementModel(new FakeLogger());

        public StatementModelBuilder AppendTransaction(Transaction transaction)
        {
            this.additionalTransactions.Add(transaction);
            return this;
        }

        public StatementModel Build()
        {
            if (this.additionalTransactions.None())
            {
                return this.model;
            }

            // IEnumerable<MethodInfo> privateMergeMethods = this.model.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(m => m.Name == "Merge");
            // MethodInfo privateMergeMethod = privateMergeMethods.First(m => m.IsPrivate);
            // privateMergeMethod.Invoke(this.model, new object[] { this.additionalTransactions });
            var additionalTransactionsModel = new StatementModel(new FakeLogger())
            {
                LastImport = this.additionalTransactions.Max(t => t.Date).Date,
            };
            additionalTransactionsModel.LoadTransactions(this.additionalTransactions);
            return this.model.Merge(additionalTransactionsModel);
        }

        public StatementModelBuilder Merge(StatementModel anotherStatementModel)
        {
            this.model.Merge(anotherStatementModel);
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
    }
}