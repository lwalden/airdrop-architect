using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Telegram.Bot;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Infrastructure.Telegram;
using AirdropArchitect.Infrastructure.Services;
using AirdropArchitect.Infrastructure.Blockchain;
using AirdropArchitect.Infrastructure.Payments;
using AirdropArchitect.Infrastructure.Data;

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

// Crypto Payment Service (Coinbase Commerce) - Optional
var coinbaseApiKey = Environment.GetEnvironmentVariable("COINBASE_COMMERCE_API_KEY");
var coinbaseWebhookSecret = Environment.GetEnvironmentVariable("COINBASE_COMMERCE_WEBHOOK_SECRET") ?? "";

if (!string.IsNullOrEmpty(coinbaseApiKey))
{
    var coinbaseProductConfig = new CoinbaseProductConfig
    {
        RevealPriceUsd = 5.00m,
        LifetimePriceUsd = 0m // Not available yet
    };

    builder.Services.AddSingleton<ICryptoPaymentService>(sp =>
    {
        var userService = sp.GetRequiredService<IUserService>();
        var logger = sp.GetRequiredService<ILogger<CoinbaseCommerceService>>();
        return new CoinbaseCommerceService(coinbaseApiKey, coinbaseWebhookSecret, coinbaseProductConfig, userService, logger);
    });
}

// User Service - Use Cosmos DB if connection string available, otherwise in-memory
var cosmosConnectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
var cosmosDatabaseName = Environment.GetEnvironmentVariable("COSMOS_DATABASE_NAME") ?? "airdrop-db";

if (!string.IsNullOrEmpty(cosmosConnectionString))
{
    // Production: Use Cosmos DB
    builder.Services.AddSingleton(sp =>
    {
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };
        return new CosmosClient(cosmosConnectionString, options);
    });

    builder.Services.AddSingleton<IUserService>(sp =>
    {
        var cosmosClient = sp.GetRequiredService<CosmosClient>();
        var logger = sp.GetRequiredService<ILogger<CosmosDbUserService>>();
        return new CosmosDbUserService(cosmosClient, cosmosDatabaseName, logger);
    });
}
else
{
    // Development: Use in-memory storage
    builder.Services.AddSingleton<IUserService, InMemoryUserService>();
}

builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();

builder.Build().Run();
