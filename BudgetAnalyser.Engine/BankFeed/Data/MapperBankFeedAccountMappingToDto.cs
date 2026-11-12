using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.BankFeed.Data;

/// <summary>
///     Maps a <see cref="BankFeedAccountMapping" /> to and from its persisted form. The account is stored by its key and resolved using the <see cref="IAccountTypeRepository" />.
/// </summary>
internal class MapperBankFeedAccountMappingToDto(IAccountTypeRepository accountTypeRepository) : IDtoMapper<BankFeedAccountMappingDto, BankFeedAccountMapping>
{
    private readonly IAccountTypeRepository accountTypeRepository = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));

    public BankFeedAccountMappingDto ToDto(BankFeedAccountMapping model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var fieldMapping = model.FieldMapping;
        return new BankFeedAccountMappingDto(
            model.ExternalAccountId,
            model.Account.Name,
            new BankFeedFieldMappingDto(
                fieldMapping.Description.ToList(),
                fieldMapping.Reference1.ToList(),
                fieldMapping.Reference2.ToList(),
                fieldMapping.Reference3.ToList(),
                fieldMapping.TransactionType.ToList()),
            fieldMapping.TypeNames.Count == 0 ? null : new Dictionary<string, string>(fieldMapping.TypeNames));
    }

    /// <exception cref="KeyNotFoundException">Thrown when the account key is not a known Budget Analyser account. Use <see cref="IsKnownAccount" /> to check first.</exception>
    public BankFeedAccountMapping ToModel(BankFeedAccountMappingDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var account = this.accountTypeRepository.GetByKey(dto.Account)
                      ?? throw new KeyNotFoundException($"The bank feed account mapping refers to an account '{dto.Account}' that does not exist.");

        BankFeedFieldMapping fieldMapping;
        if (dto.FieldMapping is null)
        {
            fieldMapping = BankFeedFieldMapping.Default with { TypeNames = CopyTypeNames(dto.TypeNames) };
        }
        else
        {
            fieldMapping = new BankFeedFieldMapping(
                dto.FieldMapping.Description?.ToList() ?? [],
                dto.FieldMapping.Reference1?.ToList() ?? [],
                dto.FieldMapping.Reference2?.ToList() ?? [],
                dto.FieldMapping.Reference3?.ToList() ?? [],
                dto.FieldMapping.TransactionType?.ToList() ?? [],
                CopyTypeNames(dto.TypeNames));
        }

        return new BankFeedAccountMapping(dto.ExternalAccountId, account, fieldMapping);
    }

    /// <summary>
    ///     Returns true if the mapping refers to an account that exists, and so can be converted with <see cref="ToModel" />.
    /// </summary>
    public bool IsKnownAccount(BankFeedAccountMappingDto dto)
    {
        return this.accountTypeRepository.GetByKey(dto.Account) is not null;
    }

    private static Dictionary<string, string> CopyTypeNames(Dictionary<string, string>? typeNames)
    {
        return typeNames is null ? new Dictionary<string, string>() : new Dictionary<string, string>(typeNames);
    }
}
