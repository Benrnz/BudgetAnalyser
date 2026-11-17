using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

public class MapperBankFeedTransactionToTransactionTest
{
    private const string ProviderKey = "test.provider";
    private static readonly ChequeAccount Cheque = new ChequeAccount(AccountTypeRepositoryConstants.Cheque);

    private readonly MapperBankFeedTransactionToTransaction subject = new(new InMemoryTransactionTypeRepository());

    [Fact]
    public void ToModel_ShouldApplyEachSourceField()
    {
        var fieldMapping = new BankFeedFieldMapping(
            [BankFeedSourceField.MerchantName],
            [BankFeedSourceField.Particulars],
            [BankFeedSourceField.Code],
            [BankFeedSourceField.Reference],
            [BankFeedSourceField.Type],
            new Dictionary<string, string>());

        var result = Map(CreateFeedTransaction(), fieldMapping);

        result.Description.ShouldBe("Test Merchant");
        result.Reference1.ShouldBe("Test Particulars");
        result.Reference2.ShouldBe("Test Code");
        result.Reference3.ShouldBe("Test Reference");
        result.TransactionType.Name.ShouldBe("EFTPOS");
    }

    [Fact]
    public void ToModel_ShouldCopyAccountDateAndAmount()
    {
        var result = Map(CreateFeedTransaction(), BankFeedFieldMapping.Default);

        result.Account.ShouldBeSameAs(Cheque);
        result.Date.ShouldBe(new DateOnly(2026, 10, 5));
        result.Amount.ShouldBe(-12.34M);
    }

    [Fact]
    public void ToModel_ShouldLeaveFieldEmpty_GivenNoSourcesOrNone()
    {
        var fieldMapping = BankFeedFieldMapping.Default with { Reference1 = [BankFeedSourceField.None] };

        var result = Map(CreateFeedTransaction(), fieldMapping);

        result.Reference1.ShouldBe(string.Empty);
        result.Reference2.ShouldBe(string.Empty);
        result.Reference3.ShouldBe(string.Empty);
    }

    [Fact]
    public void ToModel_ShouldPassTypeThrough_GivenTypeNotInTable()
    {
        var fieldMapping = BankFeedFieldMapping.Default with { TypeNames = new Dictionary<string, string> { ["DIRECT DEBIT"] = "Automatic Payment" } };

        Map(CreateFeedTransaction(), fieldMapping).TransactionType.Name.ShouldBe("EFTPOS");
    }

    [Fact]
    public void ToModel_ShouldRenameType_GivenTypeNamesTable()
    {
        var fieldMapping = BankFeedFieldMapping.Default with { TypeNames = new Dictionary<string, string> { ["EFTPOS"] = "EFTPOS TRANSACTION" } };

        Map(CreateFeedTransaction(), fieldMapping).TransactionType.Name.ShouldBe("EFTPOS TRANSACTION");
    }

    [Fact]
    public void ToModel_ShouldSetDeterministicVersion5Id()
    {
        var first = Map(CreateFeedTransaction(), BankFeedFieldMapping.Default);
        var second = Map(CreateFeedTransaction(), BankFeedFieldMapping.Default);

        first.Id.ShouldBe(second.Id);
        first.Id.ShouldBe(DeterministicGuid.CreateForBankFeedTransaction(ProviderKey, "trans_test_1"));
        first.Id.Version.ShouldBe(5);
    }

    [Fact]
    public void ToModel_ShouldUseEmptyType_GivenNoTypeAvailable()
    {
        var result = Map(CreateFeedTransaction() with { TypeName = null }, BankFeedFieldMapping.Default);

        result.TransactionType.Name.ShouldBe(NamedTransaction.Empty.Name);
    }

    [Fact]
    public void ToModel_ShouldUseFallback_GivenFirstSourceIsBlank()
    {
        var fieldMapping = BankFeedFieldMapping.Default with { Description = [BankFeedSourceField.MerchantName, BankFeedSourceField.Description] };

        var result = Map(CreateFeedTransaction() with { MerchantName = "  " }, fieldMapping);

        result.Description.ShouldBe("TEST BANK TEXT 123");
    }

    [Fact]
    public void ToModel_ShouldUseFirstSource_GivenItHasAValue()
    {
        var fieldMapping = BankFeedFieldMapping.Default with { Description = [BankFeedSourceField.MerchantName, BankFeedSourceField.Description] };

        Map(CreateFeedTransaction(), fieldMapping).Description.ShouldBe("Test Merchant");
    }

    [Theory]
    [InlineData(BankFeedSourceField.OtherPartyAccount, "00-0000-0000000-00")]
    [InlineData(BankFeedSourceField.CardSuffix, "0000")]
    [InlineData(BankFeedSourceField.Description, "TEST BANK TEXT 123")]
    public void SelectValue_ShouldReadField(BankFeedSourceField field, string expected)
    {
        MapperBankFeedTransactionToTransaction.SelectValue(CreateFeedTransaction(), [field]).ShouldBe(expected);
    }

    [Fact]
    public void FieldMappingPresets_ShouldReturnDefault_GivenAnyInstitution()
    {
        // No presets have been decided yet (Phase 6), so every institution starts with the neutral default.
        BankFeedFieldMappingPresets.ForInstitution("Test Bank A").ShouldBeSameAs(BankFeedFieldMapping.Default);
        BankFeedFieldMappingPresets.ForInstitution(null).ShouldBeSameAs(BankFeedFieldMapping.Default);
    }

    private static BankFeedTransaction CreateFeedTransaction()
    {
        return new BankFeedTransaction(
            "trans_test_1",
            "acc_test_1",
            "Test Bank A",
            new DateOnly(2026, 10, 5),
            "TEST BANK TEXT 123",
            -12.34M,
            "EFTPOS",
            "Test Particulars",
            "Test Code",
            "Test Reference",
            "00-0000-0000000-00",
            "Test Merchant",
            "0000");
    }

    private Transaction Map(BankFeedTransaction source, BankFeedFieldMapping fieldMapping)
    {
        return this.subject.ToModel(source, new BankFeedAccountMapping("acc_test_1", Cheque, fieldMapping), ProviderKey);
    }
}
