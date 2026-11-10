using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Converts a vendor-neutral <see cref="BankFeedTransaction" /> into a Budget Analyser <see cref="Transaction" />, using the account's <see cref="BankFeedFieldMapping" />.
///     This is one way only, so it does not implement <see cref="Persistence.IDtoMapper{TDto,TModel}" />.
/// </summary>
internal class MapperBankFeedTransactionToTransaction(ITransactionTypeRepository transactionTypeRepository)
{
    private readonly ITransactionTypeRepository transactionTypeRepository = transactionTypeRepository ?? throw new ArgumentNullException(nameof(transactionTypeRepository));

    /// <summary>
    ///     Gets the value of the first source field that has a value, or null if none do.
    /// </summary>
    internal static string? SelectValue(BankFeedTransaction source, IReadOnlyList<BankFeedSourceField> fields)
    {
        return fields.Select(field => ReadField(source, field)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    /// <summary>
    ///     Converts the given bank feed transaction.
    /// </summary>
    /// <param name="source">The downloaded transaction.</param>
    /// <param name="mapping">The account mapping, which supplies the account and the field mapping.</param>
    /// <param name="providerKey">The <see cref="IBankFeedProvider.ProviderKey" />, used to create the transaction's stable identifier.</param>
    public Transaction ToModel(BankFeedTransaction source, BankFeedAccountMapping mapping, string providerKey)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapping);

        var fieldMapping = mapping.FieldMapping;
        var typeName = SelectValue(source, fieldMapping.TransactionType);
        if (typeName is not null && fieldMapping.TypeNames.TryGetValue(typeName, out var renamed))
        {
            typeName = renamed;
        }

        return new Transaction
        {
            Id = DeterministicGuid.CreateForBankFeedTransaction(providerKey, source.ExternalId),
            Account = mapping.Account,
            Amount = source.Amount,
            Date = source.Date,
            Description = SelectValue(source, fieldMapping.Description) ?? string.Empty,
            Reference1 = SelectValue(source, fieldMapping.Reference1) ?? string.Empty,
            Reference2 = SelectValue(source, fieldMapping.Reference2) ?? string.Empty,
            Reference3 = SelectValue(source, fieldMapping.Reference3) ?? string.Empty,
            TransactionType = this.transactionTypeRepository.GetOrCreateNew(typeName ?? string.Empty)
        };
    }

    private static string? ReadField(BankFeedTransaction source, BankFeedSourceField field)
    {
        return field switch
        {
            BankFeedSourceField.None => null,
            BankFeedSourceField.Description => source.Description,
            BankFeedSourceField.MerchantName => source.MerchantName,
            BankFeedSourceField.Particulars => source.Particulars,
            BankFeedSourceField.Code => source.Code,
            BankFeedSourceField.Reference => source.Reference,
            BankFeedSourceField.OtherPartyAccount => source.OtherPartyAccount,
            BankFeedSourceField.Type => source.TypeName,
            BankFeedSourceField.CardSuffix => source.CardSuffix,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown bank feed source field.")
        };
    }
}
