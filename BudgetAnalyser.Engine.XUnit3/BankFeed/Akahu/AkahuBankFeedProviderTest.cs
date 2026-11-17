using System.Net;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.BankFeed.Akahu;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed.Akahu;

/// <summary>
///     Tests the Akahu adapter end to end against a fake HTTP handler serving the approved fixture files. No requests leave the machine and all values are invented.
/// </summary>
public class AkahuBankFeedProviderTest
{
    private const string FakeAppIdToken = "fake-app-id-token";
    private const string FakeUserAccessToken = "fake-user-access-token";
    private static readonly ImportDateRange November1To12 = new(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 12));

    private readonly IBankFeedCredentialStore credentialStore = Substitute.For<IBankFeedCredentialStore>();

    public AkahuBankFeedProviderTest()
    {
        this.credentialStore.Get(AkahuBankFeedProvider.Key, AkahuBankFeedProvider.AppIdTokenField).Returns(FakeAppIdToken);
        this.credentialStore.Get(AkahuBankFeedProvider.Key, AkahuBankFeedProvider.UserAccessTokenField).Returns(FakeUserAccessToken);
    }

    [Fact]
    public void AkahuCredentials_ToString_ShouldNotRevealTokens()
    {
        var text = new AkahuCredentials(FakeAppIdToken, FakeUserAccessToken).ToString();

        text.ShouldNotContain(FakeAppIdToken);
        text.ShouldNotContain(FakeUserAccessToken);
    }

    [Fact]
    public async Task AnyRequest_ShouldSendBothTokensToTheFixedAkahuAddress()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.UserDetailsResponse)));

        await CreateSubject(handler).TestConnectionAsync(CancellationToken.None);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Uri.ShouldBe(new Uri("https://api.akahu.io/v1/me"));
        request.AuthorizationScheme.ShouldBe("Bearer");
        request.AuthorizationParameter.ShouldBe(FakeUserAccessToken);
        request.AppIdHeader.ShouldBe(FakeAppIdToken);
    }

    [Fact]
    public void CredentialFields_ShouldBeSecret()
    {
        var subject = CreateSubject(new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No requests expected")));

        subject.CredentialFields.Select(f => f.Key).ShouldBe([AkahuBankFeedProvider.AppIdTokenField, AkahuBankFeedProvider.UserAccessTokenField]);
        subject.CredentialFields.ShouldAllBe(f => f.IsSecret);
        subject.ProviderKey.ShouldBe("rees.budgetanalyser.akahu");
        subject.SuggestedStartOverlapDays.ShouldBe(1);
    }

    [Fact]
    public async Task GetAccountsAsync_ShouldReturnOnlyAccountsWithTransactions()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.AccountsListResponse)));

        var result = await CreateSubject(handler).GetAccountsAsync(CancellationToken.None);

        result.Select(a => a.ExternalId).ShouldBe(["acc_test00000000000000000001", "acc_test00000000000000000002"]);
        var first = result[0];
        first.Institution.ShouldBe("Test Bank A");
        first.Name.ShouldBe("Everyday Test Account");
        first.AccountNumber.ShouldBe("99-9999-0000001-00");
        first.IsActive.ShouldBeTrue();
        first.DataAsOf.ShouldBe(DateTimeOffset.Parse("2026-11-12T18:02:15.000Z"));
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldNotCallTheApi_GivenEmptyRange()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No requests expected"));

        var result = await CreateSubject(handler).GetTransactionsAsync(
            AkahuTestDataConstants.TransactionAccountId, new ImportDateRange(new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 4)), CancellationToken.None);

        result.ShouldBeEmpty();
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldReadEveryPageWithTheSameDateRange()
    {
        var handler = CreateTwoPageHandler();

        var result = await CreateSubject(handler).GetTransactionsAsync(AkahuTestDataConstants.TransactionAccountId, November1To12, CancellationToken.None);

        result.Count.ShouldBe(5);
        var pageRequests = handler.Requests.Where(r => r.Uri.AbsolutePath.EndsWith("/transactions", StringComparison.Ordinal)).ToList();
        pageRequests.Count.ShouldBe(2);
        pageRequests[0].Uri.Query.ShouldNotContain("cursor=");
        pageRequests[1].Uri.Query.ShouldContain("cursor=test-cursor-page-2");
        foreach (var request in pageRequests)
        {
            // Start is exclusive (the last millisecond of the day before) and end is inclusive, both in NZ daylight time.
            request.Uri.Query.ShouldContain("start=2026-10-31T23%3A59%3A59.999%2B13%3A00");
            request.Uri.Query.ShouldContain("end=2026-11-12T23%3A59%3A59.999%2B13%3A00");
        }
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldMapToNewZealandDatesAndNeutralFields()
    {
        var result = await CreateSubject(CreateTwoPageHandler()).GetTransactionsAsync(AkahuTestDataConstants.TransactionAccountId, November1To12, CancellationToken.None);

        var purchase = result.Single(t => t.ExternalId == "trans_test0000000000000000001");
        purchase.Date.ShouldBe(new DateOnly(2026, 11, 12)); // 2026-11-11T11:00Z is midnight 12 Nov NZDT
        purchase.ExternalAccountId.ShouldBe(AkahuTestDataConstants.TransactionAccountId);
        purchase.Institution.ShouldBe("Test Bank A");
        purchase.Description.ShouldBe("EXAMPLE GROCER TESTVILLE");
        purchase.Amount.ShouldBe(-86.42M);
        purchase.TypeName.ShouldBe("EFTPOS");
        purchase.MerchantName.ShouldBe("Example Grocer");
        purchase.CardSuffix.ShouldBe("0001");

        var salary = result.Single(t => t.ExternalId == "trans_test0000000000000000002");
        salary.Particulars.ShouldBe("SALARY");
        salary.Code.ShouldBe("NOV");
        salary.Reference.ShouldBe("TEST0042");
        salary.OtherPartyAccount.ShouldBe("99-9999-0000002-00");

        var fee = result.Single(t => t.ExternalId == "trans_test0000000000000000004");
        fee.Particulars.ShouldBeNull();
        fee.MerchantName.ShouldBeNull();
        fee.CardSuffix.ShouldBeNull();
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldReturnNothing_GivenAFailureOnALaterPage()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (!request.RequestUri!.AbsolutePath.EndsWith("/transactions", StringComparison.Ordinal))
            {
                return FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.AccountsListResponse));
            }

            return request.RequestUri.Query.Contains("cursor=", StringComparison.Ordinal)
                ? FakeHttpMessageHandler.Json("{}", HttpStatusCode.BadRequest)
                : FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.TransactionsFirstPageResponse));
        });

        await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).GetTransactionsAsync(AkahuTestDataConstants.TransactionAccountId, November1To12, CancellationToken.None));
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldStopAfterThePageLimit()
    {
        // The first page always has a cursor, so a misbehaving API would loop forever without the limit.
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/transactions", StringComparison.Ordinal)
            ? FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.TransactionsFirstPageResponse))
            : FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.AccountsListResponse)));

        await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).GetTransactionsAsync(AkahuTestDataConstants.TransactionAccountId, November1To12, CancellationToken.None));

        handler.Requests.Count(r => r.Uri.AbsolutePath.EndsWith("/transactions", StringComparison.Ordinal)).ShouldBe(AkahuApiClient.MaxPages);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldThrow_GivenAccountNotInFeed()
    {
        var handler = CreateTwoPageHandler();

        await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).GetTransactionsAsync("acc_test_not_in_feed", November1To12, CancellationToken.None));
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldThrow_GivenNoStartDate()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No requests expected"));

        await Should.ThrowAsync<ArgumentException>(() => CreateSubject(handler).GetTransactionsAsync(
            AkahuTestDataConstants.TransactionAccountId, new ImportDateRange(null, new DateOnly(2026, 11, 12)), CancellationToken.None));
    }

    [Fact]
    public async Task RequestRefreshAsync_ShouldPostToRefreshAndReturnTrue_GivenAccepted()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"success":true}"""));

        var result = await CreateSubject(handler).RequestRefreshAsync(CancellationToken.None);

        result.ShouldBeTrue();
        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Uri.ShouldBe(new Uri("https://api.akahu.io/v1/refresh"));
        request.AuthorizationParameter.ShouldBe(FakeUserAccessToken);
        request.AppIdHeader.ShouldBe(FakeAppIdToken);
    }

    [Fact]
    public async Task RequestRefreshAsync_ShouldReturnFalseWithoutRetrying_GivenRateLimited()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"success":false,"message":"Refresh initiated within last 5 minutes"}""", HttpStatusCode.TooManyRequests));

        var result = await CreateSubject(handler).RequestRefreshAsync(CancellationToken.None);

        result.ShouldBeFalse();
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RequestRefreshAsync_ShouldThrowWithTheFeedsMessage_GivenForbidden()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"success":false,"message":"Test forbidden"}""", HttpStatusCode.Forbidden));

        var ex = await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).RequestRefreshAsync(CancellationToken.None));

        ex.Message.ShouldContain("Test forbidden");
    }

    [Fact]
    public async Task Request_ShouldRetry_GivenRateLimitedThenSuccess()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler(_ => ++calls == 1
            ? FakeHttpMessageHandler.Json("{}", HttpStatusCode.TooManyRequests)
            : FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.UserDetailsResponse)));

        var result = await CreateSubject(handler).TestConnectionAsync(CancellationToken.None);

        result.IsConnected.ShouldBeTrue();
        handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Request_ShouldGiveUpAfterMaxRetries_GivenServerErrors()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("{}", HttpStatusCode.ServiceUnavailable));

        await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).TestConnectionAsync(CancellationToken.None));

        handler.Requests.Count.ShouldBe(AkahuApiClient.MaxRetries + 1);
    }

    [Fact]
    public async Task Request_ShouldThrowAuthenticationException_GivenUnauthorised()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.UnauthorisedErrorResponse), HttpStatusCode.Unauthorized));

        var ex = await Should.ThrowAsync<BankFeedAuthenticationException>(() => CreateSubject(handler).TestConnectionAsync(CancellationToken.None));

        ex.Message.ShouldNotContain(FakeAppIdToken);
        ex.Message.ShouldNotContain(FakeUserAccessToken);
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Request_ShouldThrow_GivenSuccessFalseInAnOkResponse()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.UnauthorisedErrorResponse)));

        var ex = await Should.ThrowAsync<BankFeedException>(() => CreateSubject(handler).TestConnectionAsync(CancellationToken.None));

        ex.ShouldNotBeOfType<BankFeedAuthenticationException>();
    }

    [Fact]
    public async Task Request_ShouldThrowAuthenticationExceptionWithoutCallingTheApi_GivenNoSavedCredentials()
    {
        this.credentialStore.Get(AkahuBankFeedProvider.Key, AkahuBankFeedProvider.UserAccessTokenField).Returns((string?)null);
        var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No requests expected"));

        await Should.ThrowAsync<BankFeedAuthenticationException>(() => CreateSubject(handler).GetAccountsAsync(CancellationToken.None));

        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnUserAndHistoryAvailableFrom()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.UserDetailsResponse)));

        var result = await CreateSubject(handler).TestConnectionAsync(CancellationToken.None);

        result.IsConnected.ShouldBeTrue();
        result.ConnectedAs.ShouldBe("test.user@example.com");
        // Access granted 10 Nov 2026 (NZ); a personal app can read 730 days before that.
        result.HistoryAvailableFrom.ShouldBe(new DateOnly(2024, 11, 10));
    }

    private static FakeHttpMessageHandler CreateTwoPageHandler()
    {
        return new FakeHttpMessageHandler(request =>
        {
            if (!request.RequestUri!.AbsolutePath.EndsWith("/transactions", StringComparison.Ordinal))
            {
                return FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.AccountsListResponse));
            }

            return request.RequestUri.Query.Contains("cursor=", StringComparison.Ordinal)
                ? FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.TransactionsLastPageResponse))
                : FakeHttpMessageHandler.Json(AkahuTestDataConstants.Read(AkahuTestDataConstants.TransactionsFirstPageResponse));
        });
    }

    private AkahuBankFeedProvider CreateSubject(FakeHttpMessageHandler handler)
    {
        return new AkahuBankFeedProvider(this.credentialStore, new FakeLogger(), handler);
    }
}
