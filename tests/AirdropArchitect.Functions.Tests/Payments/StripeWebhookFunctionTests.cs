using System.Net;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Functions.Payments;
using AirdropArchitect.Functions.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace AirdropArchitect.Functions.Tests.Payments;

public class StripeWebhookFunctionTests
{
    [Fact]
    public async Task Run_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var paymentService = new FakePaymentService();
        var function = new StripeWebhookFunction(paymentService, NullLogger<StripeWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: string.Empty,
            headers: new Dictionary<string, string> { ["Stripe-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, paymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenSignatureHeaderMissing()
    {
        var paymentService = new FakePaymentService();
        var function = new StripeWebhookFunction(paymentService, NullLogger<StripeWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{\"id\":\"evt_1\"}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, paymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenProcessingFails()
    {
        var paymentService = new FakePaymentService
        {
            ProcessWebhookResult = new WebhookResult(false, "invoice.payment_failed", "user-1", "invalid event")
        };
        var function = new StripeWebhookFunction(paymentService, NullLogger<StripeWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: "{\"id\":\"evt_1\"}",
            headers: new Dictionary<string, string> { ["Stripe-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, paymentService.ProcessWebhookCallCount);
    }

    [Fact]
    public async Task Run_ReturnsOk_WhenProcessingSucceeds()
    {
        var paymentService = new FakePaymentService
        {
            ProcessWebhookResult = new WebhookResult(true, "checkout.session.completed", "user-1", null)
        };
        var function = new StripeWebhookFunction(paymentService, NullLogger<StripeWebhookFunction>.Instance);
        var ct = new CancellationTokenSource().Token;
        const string payload = "{\"id\":\"evt_1\"}";
        const string signature = "signature";
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: payload,
            headers: new Dictionary<string, string> { ["Stripe-Signature"] = signature });

        var response = await function.Run(request, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, paymentService.ProcessWebhookCallCount);
        Assert.Equal(payload, paymentService.LastPayload);
        Assert.Equal(signature, paymentService.LastSignature);
        Assert.Equal(ct, paymentService.LastCancellationToken);
    }

    [Fact]
    public async Task Run_ReturnsInternalServerError_WhenServiceThrows()
    {
        var paymentService = new FakePaymentService
        {
            ExceptionToThrow = new InvalidOperationException("boom")
        };
        var function = new StripeWebhookFunction(paymentService, NullLogger<StripeWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(
            body: "{\"id\":\"evt_1\"}",
            headers: new Dictionary<string, string> { ["Stripe-Signature"] = "signature" });

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, paymentService.ProcessWebhookCallCount);
    }

    private sealed class FakePaymentService : IPaymentService
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

        public Task<CheckoutSession> CreateSubscriptionCheckoutAsync(string userId, string tier, string successUrl, string cancelUrl, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<CheckoutSession> CreateRevealCheckoutAsync(string userId, string walletAddress, string successUrl, string cancelUrl, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task CancelSubscriptionAsync(string userId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<SubscriptionStatus?> GetSubscriptionStatusAsync(string userId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<string> CreateCustomerPortalSessionAsync(string userId, string returnUrl, CancellationToken ct = default)
            => throw new NotSupportedException();
    }
}
