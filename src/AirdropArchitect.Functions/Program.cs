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
using AirdropArchitect.Infrastructure.Payments;

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

// Payment Service (Stripe)
var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
    ?? throw new InvalidOperationException("STRIPE_SECRET_KEY environment variable is not set");
var stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") ?? "";

var stripeProductConfig = new StripeProductConfig
{
    TrackerPriceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_TRACKER") ?? "",
    ArchitectPriceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_ARCHITECT") ?? "",
    ApiPriceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_API") ?? "",
    RevealPriceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_REVEAL") ?? ""
};

builder.Services.AddSingleton<IPaymentService>(sp =>
{
    var userService = sp.GetRequiredService<IUserService>();
    var logger = sp.GetRequiredService<ILogger<StripeService>>();
    return new StripeService(stripeSecretKey, stripeWebhookSecret, stripeProductConfig, userService, logger);
});

// Services
builder.Services.AddSingleton<IUserService, InMemoryUserService>();
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

builder.Build().Run();
