using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data.Data2;

internal class MapperLedgerEntryToDto2(ILedgerBucketFactory bucketFactory, ILedgerTransactionFactory ledgerTransactionFactory) : IDtoMapper<LedgerEntryDto, LedgerEntry>
{
    private readonly ILedgerBucketFactory bucketFactory = bucketFactory ?? throw new ArgumentNullException(nameof(bucketFactory));

    private readonly IDtoMapper<LedgerTransactionDto, LedgerTransaction> transactionMapper = new MapperLedgerTransactionToDto2(ledgerTransactionFactory);

    public LedgerEntryDto ToDto(LedgerEntry model)
    {
        var dto = new LedgerEntryDto
        {
            StoredInAccount = model.LedgerBucket.StoredInAccount.Name,
            BucketCode = model.LedgerBucket.BudgetBucket.Code,
            Balance = model.Balance,
            Transactions = model.Transactions.Select(this.transactionMapper.ToDto).ToList()
        };
        return dto;
    }

    public LedgerEntry ToModel(LedgerEntryDto dto)
    {
        var entry = new LedgerEntry();

        dto.Transactions.ForEach(t =>
        {
            var ledgerTransaction = this.transactionMapper.ToModel(t);
            entry.AddTransactionForPersistenceOnly(ledgerTransaction);
        });
        // TODO shouldnt this be a ctor?
        entry.Balance = dto.Balance;
        entry.LedgerBucket = this.bucketFactory.Build(dto.BucketCode, dto.StoredInAccount);

        return entry;
    }
}
