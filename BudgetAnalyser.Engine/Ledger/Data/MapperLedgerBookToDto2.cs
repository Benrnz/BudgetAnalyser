using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Ledger.Data;

[AutoRegisterWithIoC]
internal class MapperLedgerBookToDto2 : IDtoMapper<LedgerBookDto, LedgerBook>
{
    private readonly IDtoMapper<LedgerEntryLineDto, LedgerEntryLine> ledgerEntryLineMapper;
    private readonly IDtoMapper<LedgerBucketDto, LedgerBucket> ledgerMapper;
    private readonly IDtoMapper<MobileStorageSettingsDto, MobileStorageSettings> mobileSettingsMapper;

    public MapperLedgerBookToDto2(
        IBudgetBucketRepository bucketRepo,
        IAccountTypeRepository accountTypeRepo,
        ILedgerBucketFactory bucketFactory,
        ILedgerTransactionFactory transactionFactory)
    {
        // Note AutoMapper requires a public constructor, although it's fine for the class to be internal.
        if (bucketRepo is null)
        {
            throw new ArgumentNullException(nameof(bucketRepo));
        }

        if (bucketFactory is null)
        {
            throw new ArgumentNullException(nameof(bucketFactory));
        }

        if (transactionFactory is null)
        {
            throw new ArgumentNullException(nameof(transactionFactory));
        }

        if (accountTypeRepo is null)
        {
            throw new ArgumentNullException(nameof(accountTypeRepo));
        }

        this.ledgerMapper = new MapperLedgerBucketToDto2(bucketRepo, accountTypeRepo, bucketFactory);
        this.mobileSettingsMapper = new MapperMobileSettingsToDto2();
        this.ledgerEntryLineMapper = new MapperLedgerEntryLineToDto2(accountTypeRepo, bucketFactory, transactionFactory);
    }

    public LedgerBookDto ToDto(LedgerBook model)
    {
        var ledgerBook = new LedgerBookDto
        {
            Ledgers = model.Ledgers.Select(this.ledgerMapper.ToDto).ToList(),
            MobileSettings = model.MobileSettings is null ? null : this.mobileSettingsMapper.ToDto(model.MobileSettings),
            Modified = model.Modified.ToUniversalTime(),
            Name = model.Name,
            Reconciliations = model.Reconciliations.Select(this.ledgerEntryLineMapper.ToDto).ToList(),
            StorageKey = model.StorageKey
            // Checksum is set by the repository class when saving.
        };

        return ledgerBook;
    }

    public LedgerBook ToModel(LedgerBookDto dto)
    {
        var ledgerBook = new LedgerBook(dto.Reconciliations.Select(this.ledgerEntryLineMapper.ToModel))
        {
            Modified = dto.Modified.ToLocalTime(),
            Ledgers = dto.Ledgers.Select(this.ledgerMapper.ToModel).ToList(),
            Name = dto.Name,
            MobileSettings = dto.MobileSettings is null ? null : this.mobileSettingsMapper.ToModel(dto.MobileSettings),
            StorageKey = dto.StorageKey
        };

        InitialiseAndValidateLedgerBook(ledgerBook);

        return ledgerBook;
    }

    private void InitialiseAndValidateLedgerBook(LedgerBook model)
    {
        // In this scenario, the ledger book might be new or an old one where the ledger bucket collection is missing.
        var ledgersMapWasEmpty = model.Ledgers.None();

        // If ledger column map at the book level was empty, default it to the last used ledger columns in the Dated Entries.
        if (ledgersMapWasEmpty && model.Reconciliations.Any())
        {
            model.Ledgers = model.Reconciliations.First().Entries.Select(e => e.LedgerBucket);
        }
    }
}
