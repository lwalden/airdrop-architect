using System.Net;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Functions.Payments;
using AirdropArchitect.Functions.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace AirdropArchitect.Functions.Tests.Payments;

public class CoinbaseWebhookFunctionTests
{
    [Fact]
    public async Task Run_ReturnsNotFound_WhenServiceNotConfigured()
    {
        var function = new CoinbaseWebhookFunction(null, NullLogger<CoinbaseWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{\"id\":\"evt_1\"}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var cryptoPaymentService = new FakeCryptoPaymentService();
        var function = new CoinbaseWebhookFunction(cryptoPaymentService, NullLogger<CoinbaseWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: string.Empty,
            headers: new Dictionary<string, string> { ["X-CC-Webhook-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, cryptoPaymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenSignatureHeaderMissing()
    {
        var cryptoPaymentService = new FakeCryptoPaymentService();
        var function = new CoinbaseWebhookFunction(cryptoPaymentService, NullLogger<CoinbaseWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{\"id\":\"evt_1\"}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, cryptoPaymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenProcessingFails()
    {
        var cryptoPaymentService = new FakeCryptoPaymentService
        {
            ProcessWebhookResult = new WebhookResult(false, "charge:failed", "user-1", "invalid event")
        };
        var function = new CoinbaseWebhookFunction(cryptoPaymentService, NullLogger<CoinbaseWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: "{\"id\":\"evt_1\"}",
            headers: new Dictionary<string, string> { ["X-CC-Webhook-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, cryptoPaymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsOk_WhenProcessingSucceeds()
    {
        var cryptoPaymentService = new FakeCryptoPaymentService
        {
            ProcessWebhookResult = new WebhookResult(true, "charge:confirmed", "user-1", null)
        };
        var function = new CoinbaseWebhookFunction(cryptoPaymentService, NullLogger<CoinbaseWebhookFunction>.Instance);
        var ct = new CancellationTokenSource().Token;
        const string payload = "{\"id\":\"evt_1\"}";
        const string signature = "signature";
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: payload,
            headers: new Dictionary<string, string> { ["X-CC-Webhook-Signature"] = signature });

        var response = await function.Run(request, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, cryptoPaymentService.ProcessWebhookCallCount);
        Assert.Equal(payload, cryptoPaymentService.LastPayload);
        Assert.Equal(signature, cryptoPaymentService.LastSignature);
        Assert.Equal(ct, cryptoPaymentService.LastCancellationToken);
    }

    [Fact]
    public async Task Run_ReturnsInternalServerError_WhenServiceThrows()
    {
        var cryptoPaymentService = new FakeCryptoPaymentService
        {
            ExceptionToThrow = new InvalidOperationException("boom")
        };
        var function = new CoinbaseWebhookFunction(cryptoPaymentService, NullLogger<CoinbaseWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: "{\"id\":\"evt_1\"}",
            headers: new Dictionary<string, string> { ["X-CC-Webhook-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, cryptoPaymentService.ProcessWebhookCallCount);
    }

    private sealed class FakeCryptoPaymentService : ICryptoPaymentService
    {
        public int ProcessWebhookCallCount { get; private set; }
        public string? LastPayload { get; private set; }
        public string? LastSignature { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public WebhookResult ProcessWebhookResult { get; set; } = new(true, "test.event", "user-1", null);
        public Exception? ExceptionToThrow { get; set; }

        public Task<WebhookResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default)
        {
            ProcessWebhookCallCount++;
            LastPayload = payload;
            LastSignature = signature;
            LastCancellationToken = ct;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(ProcessWebhookResult);
        }

        public Task<CryptoCharge> CreateChargeAsync(string userId, string productType, Dictionary<string, string>? metadata, string redirectUrl, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Dictionary<string, decimal> GetPricing()
            => throw new NotSupportedException();
    }
}
