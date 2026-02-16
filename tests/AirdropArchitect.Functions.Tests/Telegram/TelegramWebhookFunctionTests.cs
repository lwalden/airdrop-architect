using System.Net;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Functions.Telegram;
using AirdropArchitect.Functions.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace AirdropArchitect.Functions.Tests.Telegram;

public class TelegramWebhookFunctionTests
{
    [Fact]
    public async Task Run_ReturnsBadRequest_WhenBodyIsEmpty()
    {
        var telegramService = new FakeTelegramBotService();
        var function = new TelegramWebhookFunction(telegramService, NullLogger<TelegramWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: string.Empty);

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, telegramService.HandleUpdateCallCount);
    }

    [Fact]
    public async Task Run_ReturnsOk_WhenBodyIsValid()
    {
        var telegramService = new FakeTelegramBotService();
        var function = new TelegramWebhookFunction(telegramService, NullLogger<TelegramWebhookFunction>.Instance);
        var ct = new CancellationTokenSource().Token;
        const string payload = "{\"update_id\":123}";
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: payload);

        var response = await function.Run(request, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, telegramService.HandleUpdateCallCount);
        Assert.Equal(payload, telegramService.LastPayload);
        Assert.Equal(ct, telegramService.LastCancellationToken);
    }

    [Fact]
    public async Task Run_ReturnsInternalServerError_WhenServiceThrows()
    {
        var telegramService = new FakeTelegramBotService
        {
            ExceptionToThrow = new InvalidOperationException("boom")
        };
        var function = new TelegramWebhookFunction(telegramService, NullLogger<TelegramWebhookFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{\"update_id\":123}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, telegramService.HandleUpdateCallCount);
    }

    private sealed class FakeTelegramBotService : ITelegramBotService
    {
        public int HandleUpdateCallCount { get; private set; }
        public string? LastPayload { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task HandleUpdateAsync(string updateJson, CancellationToken cancellationToken = default)
        {
            HandleUpdateCallCount++;
            LastPayload = updateJson;
            LastCancellationToken = cancellationToken;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.CompletedTask;
        }
    }
}
