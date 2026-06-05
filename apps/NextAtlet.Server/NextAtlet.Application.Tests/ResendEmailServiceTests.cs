using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NextAtlet.Application.Common.Options;
using NextAtlet.Infrastructure.Services;
using Xunit;

namespace NextAtlet.Application.Tests;

/// <summary>
/// The Resend transport posts the right payload (recipient, sender, accept link) and is best-effort:
/// a non-success response or a transport throw is swallowed, never surfaced to the caller — the
/// invitation row is already committed.
/// </summary>
public class ResendEmailServiceTests
{
    private static readonly Guid InvitationId = new("33333333-3333-3333-3333-333333333333");

    private static EmailOptions Options() => new()
    {
        ApiKey = "re_test_key",
        FromAddress = "no-reply@nextatlet.dk",
        FromName = "NextAtlet",
        AppBaseUrl = "https://app.nextatlet.dk"
    };

    private static ResendEmailService Build(StubHandler handler)
    {
        var options = Options();
        // Mirror the production typed-client setup (base address + bearer auth from Program.cs).
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") };
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        return new ResendEmailService(client, new OptionsWrapper<EmailOptions>(options), NullLogger<ResendEmailService>.Instance);
    }

    [Fact]
    public async Task Sends_expected_payload_with_accept_link()
    {
        var handler = new StubHandler(HttpStatusCode.OK, """{ "id": "abc" }""");
        var service = Build(handler);

        await service.SendInviteAsync("receiver@example.com", InvitationId);

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://api.resend.com/emails", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.Request.Headers.Authorization!.Scheme);
        Assert.Equal("re_test_key", handler.Request.Headers.Authorization.Parameter);

        using var json = JsonDocument.Parse(handler.Body!);
        var root = json.RootElement;
        Assert.Equal("NextAtlet <no-reply@nextatlet.dk>", root.GetProperty("from").GetString());
        Assert.Equal("receiver@example.com", root.GetProperty("to")[0].GetString());

        var expectedLink = $"https://app.nextatlet.dk/invitations/{InvitationId}/accept";
        Assert.Contains(expectedLink, root.GetProperty("html").GetString());
        Assert.Contains(expectedLink, root.GetProperty("text").GetString());
    }

    [Fact]
    public async Task Non_success_response_is_swallowed()
    {
        var service = Build(new StubHandler(HttpStatusCode.UnprocessableEntity, """{ "message": "invalid from" }"""));

        // Best-effort delivery: must not throw even though Resend rejected the request.
        await service.SendInviteAsync("receiver@example.com", InvitationId);
    }

    [Fact]
    public async Task Transport_failure_is_swallowed()
    {
        var service = Build(new StubHandler(new HttpRequestException("network down")));

        await service.SendInviteAsync("receiver@example.com", InvitationId);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _responseBody = "";
        private readonly Exception? _throw;

        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        public StubHandler(HttpStatusCode status, string responseBody)
        {
            _status = status;
            _responseBody = responseBody;
        }

        public StubHandler(Exception toThrow) => _throw = toThrow;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content is not null)
                Body = await request.Content.ReadAsStringAsync(cancellationToken);

            if (_throw is not null)
                throw _throw;

            return new HttpResponseMessage(_status) { Content = new StringContent(_responseBody) };
        }
    }
}
