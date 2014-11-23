using System;
using System.Collections.Generic;
using System.IO;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    public interface ILedgerService
    {
        LedgerBook DisplayLedgerBook([NotNull] string fileName, [NotNull] BudgetModel budget, [NotNull] StatementModel statement);

        LedgerEntryLine MonthEndReconciliation(DateTime date, IEnumerable<BankBalance> balances, bool ignoreWarnings = false);

        void RemoveReconciliation(LedgerEntryLine line);

        void RenameLedgerBook(LedgerBook ledgerBook, string newName);

        LedgerColumn TrackNewBudgetBucket(ExpenseBucket bucket, AccountType storeInThisAccount);

        LedgerEntryLine UnlockCurrentMonth();
    }

    [AutoRegisterWithIoC]
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerBookRepository ledgerRepository;
        private LedgerBook book;
        private BudgetModel budgetModel;
        private StatementModel statementModel;

        public LedgerService([NotNull] ILedgerBookRepository ledgerRepository)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException("ledgerRepository");
            }

            this.ledgerRepository = ledgerRepository;
        }

        public LedgerBook DisplayLedgerBook(string fileName, BudgetModel budget, StatementModel statement)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }
            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (!this.ledgerRepository.Exists(fileName))
            {
                throw new FileNotFoundException("The requested file, or the previously loaded file, cannot be located.\n" + fileName, fileName);
            }

            this.book = this.ledgerRepository.Load(fileName);
            this.budgetModel = budget;
            this.statementModel = statement;
            return this.book;
        }

        public LedgerEntryLine MonthEndReconciliation(DateTime date, IEnumerable<BankBalance> balances, bool ignoreWarnings = false)
        {
            return this.book.Reconcile(date, balances, this.budgetModel, this.statementModel, ignoreWarnings);
        }

        public void RemoveReconciliation(LedgerEntryLine line)
        {
            throw new NotImplementedException();
        }

        public void RenameLedgerBook(LedgerBook ledgerBook, string newName)
        {
            ledgerBook.Name = newName;
        }

        public LedgerColumn TrackNewBudgetBucket(ExpenseBucket bucket, AccountType storeInThisAccount)
        {
            return this.book.AddLedger(bucket, storeInThisAccount);
        }

        public LedgerEntryLine UnlockCurrentMonth()
        {
            throw new NotImplementedException();
        }
    }
}