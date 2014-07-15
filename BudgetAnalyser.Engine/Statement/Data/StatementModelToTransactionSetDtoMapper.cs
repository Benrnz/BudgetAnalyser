using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC]
    public class StatementModelToTransactionSetDtoMapper : IStatementModelToTransactionSetDtoMapper
    {
        private readonly TransactionToTransactionDtoMapper transactionMapper;

        public StatementModelToTransactionSetDtoMapper([NotNull] TransactionToTransactionDtoMapper transactionMapper)
        {
            if (transactionMapper == null)
            {
                throw new ArgumentNullException("transactionMapper");
            }

            this.transactionMapper = transactionMapper;
        }

        public TransactionSetDto Map([NotNull] StatementModel model, string versionHash, string fileName)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new TransactionSetDto
            {
                LastImport = model.LastImport,
                Transactions = model.AllTransactions.Select(t => this.transactionMapper.Map(t)).ToList(),
                VersionHash = versionHash,
                FileName = fileName,
            };
        }
    }
}