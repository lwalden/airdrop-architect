using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AirdropArchitect.Core.Interfaces;

namespace AirdropArchitect.Functions.Payments;

public class StripeWebhookFunction
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<StripeWebhookFunction> _logger;

    public StripeWebhookFunction(
        IPaymentService paymentService,
        ILogger<StripeWebhookFunction> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [Function("StripeWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/stripe/webhook")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Received empty Stripe webhook body");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Get the Stripe signature header
            var signature = req.Headers.TryGetValues("Stripe-Signature", out var values)
                ? values.FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Missing Stripe-Signature header");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Processing Stripe webhook");
            var result = await _paymentService.ProcessWebhookAsync(body, signature, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Stripe webhook processing failed: {EventType} - {Error}",
                    result.EventType, result.ErrorMessage);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Stripe webhook processed successfully: {EventType}", result.EventType);
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
