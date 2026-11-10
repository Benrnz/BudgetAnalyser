using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BudgetAnalyser.Engine.BankFeed.Akahu.Data;

namespace BudgetAnalyser.Engine.BankFeed.Akahu;

/// <summary>
///     A thin wrapper around the Akahu REST API. Requests are authenticated with the supplied <see cref="AkahuCredentials" />, which are never logged.
///     Rate limited (429) and server error (5xx) responses are retried with exponential backoff. All other failures are translated into <see cref="BankFeedException" />s.
/// </summary>
internal sealed class AkahuApiClient : IDisposable
{
    /// <summary>
    ///     The Akahu API address. This is fixed in code, not a setting, so the tokens can never be redirected to another host.
    /// </summary>
    internal const string BaseAddress = "https://api.akahu.io/v1/";

    /// <summary>
    ///     The maximum number of pages read for one account download before giving up. At 100 transactions per page this allows 10,000 transactions.
    /// </summary>
    internal const int MaxPages = 100;

    /// <summary>
    ///     The maximum number of retries after a rate limited (429) or server error (5xx) response.
    /// </summary>
    internal const int MaxRetries = 5;

    private const string AppIdHeader = "X-Akahu-Id";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryBackoffBase = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     Akahu uses snake_case property names. The underscore prefixed identifiers are mapped explicitly on the DTOs.
    /// </summary>
    internal static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private readonly HttpClient httpClient;
    private readonly ILogger logger;

    public AkahuApiClient(HttpMessageHandler handler, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(handler);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress), Timeout = RequestTimeout };
    }

    public void Dispose()
    {
        this.httpClient.Dispose();
    }

    /// <summary>
    ///     GET /v1/accounts
    /// </summary>
    public async Task<IReadOnlyList<AkahuAccountDto>> GetAccountsAsync(AkahuCredentials credentials, CancellationToken cancellationToken)
    {
        var response = await GetAsync<AkahuAccountsResponseDto>("accounts", credentials, cancellationToken);
        return response.Items ?? [];
    }

    /// <summary>
    ///     Gets every settled transaction for an account for the given inclusive range of New Zealand local dates. Every page is read before returning; if any request fails
    ///     an exception is thrown and nothing is returned.
    /// </summary>
    public async Task<IReadOnlyList<AkahuTransactionDto>> GetAllTransactionsAsync(
        string accountId,
        DateOnly firstDate,
        DateOnly lastDate,
        AkahuCredentials credentials,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId);

        // The same start and end must be sent with every page request.
        var (start, end) = AkahuDates.ToQueryRange(firstDate, lastDate);
        var baseQuery = $"accounts/{Uri.EscapeDataString(accountId)}/transactions?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}";

        var transactions = new List<AkahuTransactionDto>();
        string? cursor = null;
        for (var page = 1; page <= MaxPages; page++)
        {
            // Never send a null cursor back to the API, it responds with an error.
            var uri = cursor is null ? baseQuery : $"{baseQuery}&cursor={Uri.EscapeDataString(cursor)}";
            var response = await GetAsync<AkahuTransactionsPageDto>(uri, credentials, cancellationToken);
            transactions.AddRange(response.Items ?? []);
            cursor = response.Cursor?.Next;
            if (cursor is null)
            {
                this.logger.LogInfo(_ => $"Bank feed: downloaded {transactions.Count} transactions in {page} page(s) for {firstDate:yyyy-MM-dd} to {lastDate:yyyy-MM-dd}.");
                return transactions;
            }
        }

        throw new BankFeedException($"The bank feed returned more than {MaxPages} pages of transactions. Try a shorter date range. Nothing was imported.");
    }

    /// <summary>
    ///     GET /v1/me
    /// </summary>
    public async Task<AkahuMeDto> GetMeAsync(AkahuCredentials credentials, CancellationToken cancellationToken)
    {
        var response = await GetAsync<AkahuMeResponseDto>("me", credentials, cancellationToken);
        return response.Item ?? throw new BankFeedException("The bank feed did not return the user details.");
    }

    private static bool IsRetryable(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }

    private static TimeSpan RetryDelay(int retryNumber)
    {
        // Exponential backoff with random jitter between 0.75 and 1.25, as recommended by Akahu: 100ms, 200ms, 400ms, 800ms, 1.6s.
        var jitter = 0.75 + (Random.Shared.NextDouble() / 2);
        return TimeSpan.FromMilliseconds(RetryBackoffBase.TotalMilliseconds * Math.Pow(2, retryNumber) * jitter);
    }

    private async Task<T> GetAsync<T>(string relativeUri, AkahuCredentials credentials, CancellationToken cancellationToken) where T : AkahuResponseDto
    {
        ArgumentNullException.ThrowIfNull(credentials);

        for (var retryNumber = 0; ; retryNumber++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.UserAccessToken);
            request.Headers.Add(AppIdHeader, credentials.AppIdToken);

            HttpResponseMessage response;
            try
            {
                response = await this.httpClient.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new BankFeedException($"The bank feed did not respond within {RequestTimeout.TotalSeconds:N0} seconds.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new BankFeedException($"Unable to contact the bank feed. {ex.Message}", ex);
            }

            using (response)
            {
                if (IsRetryable(response.StatusCode) && retryNumber < MaxRetries)
                {
                    var delay = RetryDelay(retryNumber);
                    var statusCode = (int)response.StatusCode;
                    this.logger.LogWarning(_ => $"Bank feed responded with HTTP {statusCode}, retrying in {delay.TotalMilliseconds:N0}ms.");
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new BankFeedAuthenticationException("The bank feed rejected the saved credentials. They may have been regenerated or revoked.");
                }

                var dto = await ReadJsonAsync<T>(response, cancellationToken);
                if (!response.IsSuccessStatusCode || dto is null || !dto.Success)
                {
                    var message = string.IsNullOrWhiteSpace(dto?.Message) ? response.ReasonPhrase : dto.Message;
                    throw new BankFeedException($"The bank feed returned an error (HTTP {(int)response.StatusCode}): {message}");
                }

                return dto;
            }
        }
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : AkahuResponseDto
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            if (!response.IsSuccessStatusCode)
            {
                // Error responses are not always JSON; the status code is reported instead.
                return null;
            }

            throw new BankFeedException("The bank feed returned data in an unexpected format.", ex);
        }
    }
}
