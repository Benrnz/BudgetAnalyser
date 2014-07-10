using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC]
    public class TransactionSetDtoToStatementModelMapper : ITransactionSetDtoToStatementModelMapper
    {
        private readonly ILogger logger;
        private readonly TransactionDtoToTransactionMapper transactionMapper;

        public TransactionSetDtoToStatementModelMapper([NotNull] ILogger logger, [NotNull] TransactionDtoToTransactionMapper transactionMapper)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (transactionMapper == null)
            {
                throw new ArgumentNullException("transactionMapper");
            }

            this.logger = logger;
            this.transactionMapper = transactionMapper;
        }

        public StatementModel Map(TransactionSetDto setDto)
        {
            var model = new StatementModel(this.logger)
            {
                FileName = setDto.FileName,
                LastImport = setDto.LastImport,
            };

            model.LoadTransactions(setDto.Transactions.Select(t => this.transactionMapper.Map(t)));

            return model;
        }
    }
}
