using System.Text.Json;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.BankFeed.Data;
using BudgetAnalyser.Engine.Persistence;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

/// <summary>
///     Checks the bank feed account mappings are saved in, and loaded from, the main Budget Analyser file.
/// </summary>
public class BankFeedAccountMappingPersistenceTest
{
    private readonly MapperBankFeedAccountMappingToDto mappingMapper = new(new InMemoryAccountTypeRepository());

    [Fact]
    public void ApplicationDatabaseClose_ShouldClearMappings()
    {
        var database = new ApplicationDatabase { BankFeedAccountMappings = [new BankFeedAccountMappingDto("acc_test_1", "CHEQUE")] };

        database.Close();

        database.BankFeedAccountMappings.ShouldBeEmpty();
    }

    [Fact]
    public void Json_ShouldLoadEmptyList_GivenFileSavedBeforeBankFeeds()
    {
        const string olderFile = """
                                 {
                                   "BudgetCollectionRootDto": "Budget.json",
                                   "Filter": {},
                                   "LedgerBookRootDto": "Ledger.json",
                                   "MatchingRulesCollectionRootDto": "Rules.json",
                                   "TransactionsCollectionRootDto": "Transactions.csv",
                                   "WidgetCollectionRootDto": "Widgets.json"
                                 }
                                 """;

        var dto = JsonSerializer.Deserialize<BudgetAnalyserStorageRoot2>(olderFile)!;
        var model = new MapperApplicationDatabaseToStorageRoot3().ToModel(dto);

        model.BankFeedAccountMappings.ShouldBeEmpty();
    }

    [Fact]
    public void Json_ShouldNotWriteMappings_GivenNone()
    {
        var json = JsonSerializer.Serialize(new MapperApplicationDatabaseToStorageRoot3().ToDto(new ApplicationDatabase()));

        json.ShouldNotContain(nameof(BudgetAnalyserStorageRoot2.BankFeedAccountMappings));
    }

    [Fact]
    public void Json_ShouldRoundTripMappings_ThroughStorageRoot()
    {
        var database = new ApplicationDatabase
        {
            BankFeedAccountMappings = [this.mappingMapper.ToDto(CreateMapping())]
        };

        var json = JsonSerializer.Serialize(new MapperApplicationDatabaseToStorageRoot3().ToDto(database));
        var reloaded = new MapperApplicationDatabaseToStorageRoot3().ToModel(JsonSerializer.Deserialize<BudgetAnalyserStorageRoot2>(json)!);
        var result = this.mappingMapper.ToModel(reloaded.BankFeedAccountMappings.Single());

        result.ExternalAccountId.ShouldBe("acc_test_1");
        result.Account.Name.ShouldBe(AccountTypeRepositoryConstants.Visa);
        result.FieldMapping.Description.ShouldBe([BankFeedSourceField.MerchantName, BankFeedSourceField.Description]);
        result.FieldMapping.Reference1.ShouldBe([BankFeedSourceField.Reference]);
        result.FieldMapping.Reference2.ShouldBeEmpty();
        result.FieldMapping.Reference3.ShouldBe([BankFeedSourceField.Code]);
        result.FieldMapping.TransactionType.ShouldBe([BankFeedSourceField.Type]);
        result.FieldMapping.TypeNames["EFTPOS"].ShouldBe("EFTPOS TRANSACTION");
    }

    [Fact]
    public void Json_ShouldStoreOnlyIdAccountKeyAndFieldNames()
    {
        var dto = this.mappingMapper.ToDto(CreateMapping());

        var json = JsonSerializer.Serialize(dto);

        json.ShouldBe(
            """{"ExternalAccountId":"acc_test_1","Account":"VISA","FieldMapping":{"Description":["MerchantName","Description"],"Reference1":["Reference"],"Reference2":[],"Reference3":["Code"],"TransactionType":["Type"]},"TypeNames":{"EFTPOS":"EFTPOS TRANSACTION"}}""");
    }

    [Fact]
    public void Json_ShouldWriteNoOptionalSections_GivenMinimalMapping()
    {
        var json = JsonSerializer.Serialize(new BankFeedAccountMappingDto("acc_test_1", "CHEQUE"));

        json.ShouldBe("""{"ExternalAccountId":"acc_test_1","Account":"CHEQUE"}""");
    }

    [Fact]
    public void ToModel_ShouldUseDefaultFieldMapping_GivenNoFieldMappingSaved()
    {
        var dto = JsonSerializer.Deserialize<BankFeedAccountMappingDto>("""{"ExternalAccountId":"acc_test_1","Account":"CHEQUE"}""")!;

        var result = this.mappingMapper.ToModel(dto);

        result.FieldMapping.Description.ShouldBe(BankFeedFieldMapping.Default.Description);
        result.FieldMapping.TransactionType.ShouldBe(BankFeedFieldMapping.Default.TransactionType);
        result.FieldMapping.Reference1.ShouldBeEmpty();
        result.FieldMapping.TypeNames.ShouldBeEmpty();
    }

    [Fact]
    public void ToModel_ShouldUseEmptyList_GivenAListIsMissing()
    {
        var dto = JsonSerializer.Deserialize<BankFeedAccountMappingDto>("""{"ExternalAccountId":"acc_test_1","Account":"CHEQUE","FieldMapping":{"Description":["Description"]}}""")!;

        var result = this.mappingMapper.ToModel(dto);

        result.FieldMapping.Description.ShouldBe([BankFeedSourceField.Description]);
        result.FieldMapping.TransactionType.ShouldBeEmpty();
    }

    [Fact]
    public void ToModel_ShouldThrow_GivenUnknownAccount()
    {
        var dto = new BankFeedAccountMappingDto("acc_test_1", "NOSUCHACCOUNT");

        this.mappingMapper.IsKnownAccount(dto).ShouldBeFalse();
        Should.Throw<KeyNotFoundException>(() => this.mappingMapper.ToModel(dto));
    }

    private static BankFeedAccountMapping CreateMapping()
    {
        return new BankFeedAccountMapping(
            "acc_test_1",
            new VisaAccount(AccountTypeRepositoryConstants.Visa),
            new BankFeedFieldMapping(
                [BankFeedSourceField.MerchantName, BankFeedSourceField.Description],
                [BankFeedSourceField.Reference],
                [],
                [BankFeedSourceField.Code],
                [BankFeedSourceField.Type],
                new Dictionary<string, string> { ["EFTPOS"] = "EFTPOS TRANSACTION" }));
    }
}
