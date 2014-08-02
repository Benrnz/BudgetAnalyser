using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<TransactionSetDto, StatementModel>))]
    public class TransactionSetDtoToStatementModelMapper : BasicMapper<TransactionSetDto, StatementModel>
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

        public override StatementModel Map([NotNull] TransactionSetDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var model = new StatementModel(this.logger)
            {
                FileName = source.FileName,
                LastImport = source.LastImport,
            };

            model.LoadTransactions(source.Transactions.Select(t => this.transactionMapper.Map(t)));

            return model;
        }
    }
}
