using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Mobile;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data.Data2;

[AutoRegisterWithIoC]
public class MapperLedgerBookToDto2 : IDtoMapper<LedgerBookDto, LedgerBook>
{
    private readonly IAccountTypeRepository accountTypeRepo;
    private readonly Dictionary<string, LedgerBucket> cachedLedgers = new();
    private readonly IDtoMapper<LedgerEntryLineDto, LedgerEntryLine> ledgerEntryLineMapper;
    private readonly IDtoMapper<LedgerBucketDto, LedgerBucket> ledgerMapper;
    private readonly IDtoMapper<MobileStorageSettingsDto, MobileStorageSettings> mobileSettingsMapper;

    internal MapperLedgerBookToDto2(
        IBudgetBucketRepository bucketRepo,
        IAccountTypeRepository accountTypeRepo,
        ILedgerBucketFactory bucketFactory,
        ILedgerTransactionFactory transactionFactory)
    {
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

        this.accountTypeRepo = accountTypeRepo ?? throw new ArgumentNullException(nameof(accountTypeRepo));
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
            Modified = model.Modified,
            Name = model.Name,
            Reconciliations = model.Reconciliations.Select(this.ledgerEntryLineMapper.ToDto).ToList(),
            StorageKey = model.StorageKey
            // Checksum is set by the repository class when saving.
        };

        return ledgerBook;
    }

    public LedgerBook ToModel(LedgerBookDto dto)
    {
        var ledgerBook = new LedgerBook
        {
            Modified = dto.Modified,
            Ledgers = dto.Ledgers.Select(this.ledgerMapper.ToModel).ToList(),
            Name = dto.Name,
            MobileSettings = dto.MobileSettings is null ? null : this.mobileSettingsMapper.ToModel(dto.MobileSettings),
            StorageKey = dto.StorageKey
        };

        ledgerBook.SetReconciliations(dto.Reconciliations.Select(this.ledgerEntryLineMapper.ToModel).ToList());

        InitialiseAndValidateLedgerBook(ledgerBook);

        return ledgerBook;
    }

    private LedgerBucket GetOrAddLedgerFromCache(LedgerBucket ledger, bool throwIfNotFound = false)
    {
        if (this.cachedLedgers.TryGetValue(ledger.BudgetBucket.Code, out var cachedLedger))
        {
            return cachedLedger;
        }

        if (throwIfNotFound)
        {
            throw new ArgumentException($"Ledger Bucket {ledger.BudgetBucket.Code} not found in cache.");
        }

        this.cachedLedgers.Add(ledger.BudgetBucket.Code, ledger);
        return ledger;
    }

    private void InitialiseAndValidateLedgerBook(LedgerBook model)
    {
        this.cachedLedgers.Clear();
        foreach (var ledgerBucket in model.Ledgers)
        {
            if (ledgerBucket.StoredInAccount is null)
            {
                // Defaults to Cheque Account if unspecified.
                ledgerBucket.StoredInAccount = this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
            }

            GetOrAddLedgerFromCache(ledgerBucket);
        }

        var ledgersMapWasEmpty = model.Ledgers.None();

        // Default to CHEQUE when StoredInAccount is null.
        foreach (var line in model.Reconciliations)
        {
            foreach (var entry in line.Entries)
            {
                // Ensure the ledger bucket is the same instance as listed in the book.Ledgers;
                // If its not found thats ok, this means its a old ledger no longer declared in the LedgerBook and is archived and hidden.
                entry.LedgerBucket = GetOrAddLedgerFromCache(entry.LedgerBucket);
                if (entry.LedgerBucket is not null && entry.LedgerBucket.StoredInAccount is null)
                {
                    entry.LedgerBucket.StoredInAccount = this.accountTypeRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                }
            }
        }

        // If ledger column map at the book level was empty, default it to the last used ledger columns in the Dated Entries.
        if (ledgersMapWasEmpty && model.Reconciliations.Any())
        {
            model.Ledgers = model.Reconciliations.First().Entries.Select(e => e.LedgerBucket);
        }
    }
}
