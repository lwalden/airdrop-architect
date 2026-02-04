using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Infrastructure.Telegram;
using AirdropArchitect.Infrastructure.Services;
using AirdropArchitect.Infrastructure.Blockchain;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Telegram Bot
var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
    ?? throw new InvalidOperationException("TELEGRAM_BOT_TOKEN environment variable is not set");

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramToken));

// Blockchain Service (Alchemy)
var alchemyApiKey = Environment.GetEnvironmentVariable("ALCHEMY_API_KEY")
    ?? throw new InvalidOperationException("ALCHEMY_API_KEY environment variable is not set");

builder.Services.AddSingleton<IBlockchainService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AlchemyService>>();
    return new AlchemyService(alchemyApiKey, logger);
});

// Services
builder.Services.AddSingleton<IUserService, InMemoryUserService>();
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

builder.Build().Run();
