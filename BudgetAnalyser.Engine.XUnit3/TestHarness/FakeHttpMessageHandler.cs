using System.Net;
using System.Text;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

/// <summary>
///     A fake HTTP handler so no requests ever leave the machine. Each request is answered by the supplied function and recorded for assertions.
/// </summary>
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<RecordedRequest> Requests { get; } = [];

    public static HttpResponseMessage Json(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var appId = request.Headers.TryGetValues("X-Akahu-Id", out var values) ? values.Single() : null;
        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, request.Headers.Authorization?.Scheme, request.Headers.Authorization?.Parameter, appId));
        return Task.FromResult(respond(request));
    }

    /// <summary>
    ///     A copy of the parts of a request the tests check. The request itself is disposed by the client once sent.
    /// </summary>
    internal sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? AuthorizationScheme, string? AuthorizationParameter, string? AppIdHeader);
}
