using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AirdropArchitect.Core.Interfaces;

namespace AirdropArchitect.Functions.Payments;

public class CoinbaseWebhookFunction
{
    private readonly ICryptoPaymentService? _cryptoPaymentService;
    private readonly ILogger<CoinbaseWebhookFunction> _logger;

    public CoinbaseWebhookFunction(
        ICryptoPaymentService? cryptoPaymentService,
        ILogger<CoinbaseWebhookFunction> logger)
    {
        _cryptoPaymentService = cryptoPaymentService;
        _logger = logger;
    }

    [Function("CoinbaseWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/coinbase/webhook")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        if (_cryptoPaymentService == null)
        {
            _logger.LogWarning("Coinbase Commerce not configured, rejecting webhook");
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        try
        {
            var body = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Received empty Coinbase webhook body");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Get the webhook signature header
            var signature = req.Headers.TryGetValues("X-CC-Webhook-Signature", out var values)
                ? values.FirstOrDefault()
                : null;

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Missing X-CC-Webhook-Signature header");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Processing Coinbase Commerce webhook");
            var result = await _cryptoPaymentService.ProcessWebhookAsync(body, signature, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Coinbase webhook processing failed: {EventType} - {Error}",
                    result.EventType, result.ErrorMessage);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Coinbase webhook processed successfully: {EventType}", result.EventType);
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Coinbase webhook");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
