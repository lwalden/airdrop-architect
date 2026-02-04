using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AirdropArchitect.Core.Interfaces;

namespace AirdropArchitect.Functions.Telegram;

public class TelegramWebhookFunction
{
    private readonly ITelegramBotService _telegramService;
    private readonly ILogger<TelegramWebhookFunction> _logger;

    public TelegramWebhookFunction(
        ITelegramBotService telegramService,
        ILogger<TelegramWebhookFunction> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    [Function("TelegramWebhook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "telegram/webhook")]
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(body))
            {
                _logger.LogWarning("Received empty webhook body");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Processing Telegram webhook");
            await _telegramService.HandleUpdateAsync(body, cancellationToken);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram webhook");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
