using System.Text.Json;
using BudgetAnalyser.Engine.BankFeed.Akahu;
using BudgetAnalyser.Engine.BankFeed.Akahu.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed.Akahu;

/// <summary>
///     Checks the Akahu DTOs read the approved fixture files correctly, and the mappers produce the vendor-neutral records.
/// </summary>
public class AkahuDtoMappingTest
{
    [Fact]
    public void AccountMapper_ShouldMapInactiveAccountWithoutTransactions()
    {
        var accounts = Deserialise<AkahuAccountsResponseDto>(AkahuTestDataConstants.AccountsListResponse).Items!;
        var investment = accounts.Single(a => a.Id == "acc_test00000000000000000003");

        MapperAkahuAccountToBankFeedAccount.SupportsTransactions(investment).ShouldBeFalse();
        var result = new MapperAkahuAccountToBankFeedAccount().ToModel(investment);
        result.IsActive.ShouldBeFalse();
        result.AccountNumber.ShouldBeNull();
        result.DataAsOf.ShouldBeNull();
        result.Institution.ShouldBe("Test Fund C");
    }

    [Fact]
    public void AccountsListResponse_ShouldDeserialiseAllAccounts()
    {
        var response = Deserialise<AkahuAccountsResponseDto>(AkahuTestDataConstants.AccountsListResponse);

        response.Success.ShouldBeTrue();
        response.Items!.Length.ShouldBe(3);
        var card = response.Items[1];
        card.Connection!.Name.ShouldBe("Test Bank B");
        card.Connection.ConnectionType.ShouldBe("official");
        card.Type.ShouldBe("CREDITCARD");
        card.Balance!.Current.ShouldBe(-842.10M);
        card.Balance.Limit.ShouldBe(10000M);
    }

    [Fact]
    public void TransactionMapper_ShouldUseEmptyDescription_GivenNoDescription()
    {
        var dto = new AkahuTransactionDto("trans_test_no_description", "acc_test_account", null, null, null, null, DateTimeOffset.Parse("2026-11-01T11:00:00Z"), null, -1M, null, null, null, null);

        var result = new MapperAkahuTransactionToBankFeedTransaction().ToModel(dto, "Test Bank A");

        result.Description.ShouldBe(string.Empty);
        result.TypeName.ShouldBeNull();
        result.Date.ShouldBe(new DateOnly(2026, 11, 2));
    }

    [Fact]
    public void TransactionsLastPageResponse_ShouldHaveNullCursorAndOptionalFields()
    {
        var response = Deserialise<AkahuTransactionsPageDto>(AkahuTestDataConstants.TransactionsLastPageResponse);

        response.Cursor!.Next.ShouldBeNull();
        response.Items!.Length.ShouldBe(2);
        var foreign = response.Items[1];
        foreign.Migrated.ShouldBe("trans_test0000000000000000099");
        foreign.Meta!.Conversion!.Currency.ShouldBe("USD");
        foreign.Meta.Conversion.Amount.ShouldBe(18.99M);
        response.Items[0].Meta.ShouldBeNull();
        response.Items[0].Balance.ShouldBeNull();
    }

    [Fact]
    public void UnauthorisedErrorResponse_ShouldDeserialiseSuccessFalseAndMessage()
    {
        var response = Deserialise<AkahuMeResponseDto>(AkahuTestDataConstants.UnauthorisedErrorResponse);

        response.Success.ShouldBeFalse();
        response.Message.ShouldBe("Unauthorized");
        response.Item.ShouldBeNull();
    }

    [Fact]
    public void UserDetailsResponse_ShouldDeserialiseSnakeCaseFields()
    {
        var response = Deserialise<AkahuMeResponseDto>(AkahuTestDataConstants.UserDetailsResponse);

        response.Item!.Id.ShouldBe("user_test00000000000000000001");
        response.Item.AccessGrantedAt.ShouldBe(DateTimeOffset.Parse("2026-11-10T03:15:42.120Z"));
        response.Item.Email.ShouldBe("test.user@example.com");
    }

    private static T Deserialise<T>(string resourceName)
    {
        return JsonSerializer.Deserialize<T>(AkahuTestDataConstants.Read(resourceName), AkahuApiClient.JsonOptions)!;
    }
}
