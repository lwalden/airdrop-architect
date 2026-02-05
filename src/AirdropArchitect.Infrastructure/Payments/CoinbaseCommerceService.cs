using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Payments;

/// <summary>
/// Coinbase Commerce implementation for one-time crypto payments
/// </summary>
public class CoinbaseCommerceService : ICryptoPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IUserService _userService;
    private readonly ILogger<CoinbaseCommerceService> _logger;
    private readonly string _webhookSecret;
    private readonly Dictionary<string, CryptoProduct> _products;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CoinbaseCommerceService(
        string apiKey,
        string webhookSecret,
        CoinbaseProductConfig productConfig,
        IUserService userService,
        ILogger<CoinbaseCommerceService> logger)
    {
        _webhookSecret = webhookSecret;
        _userService = userService;
        _logger = logger;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.commerce.coinbase.com/")
        };
        _httpClient.DefaultRequestHeaders.Add("X-CC-Api-Key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("X-CC-Version", "2018-03-22");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _products = new Dictionary<string, CryptoProduct>(StringComparer.OrdinalIgnoreCase)
        {
            ["reveal"] = new CryptoProduct("Wallet Reveal", "Unlock detailed wallet analysis", productConfig.RevealPriceUsd)
        };

        // Future products can be added here
        if (productConfig.LifetimePriceUsd > 0)
        {
            _products["lifetime"] = new CryptoProduct("Lifetime Membership", "Lifetime access to all features", productConfig.LifetimePriceUsd);
        }
    }

    public async Task<CryptoCharge> CreateChargeAsync(
        string userId,
        string productType,
        Dictionary<string, string>? metadata,
        string redirectUrl,
        CancellationToken ct = default)
    {
        if (!_products.TryGetValue(productType, out var product))
        {
            throw new ArgumentException($"Unknown product type: {productType}");
        }

        var chargeMetadata = new Dictionary<string, string>
        {
            ["user_id"] = userId,
            ["product_type"] = productType
        };

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                chargeMetadata[kvp.Key] = kvp.Value;
            }
        }

        var request = new CreateChargeRequest
        {
            Name = product.Name,
            Description = product.Description,
            PricingType = "fixed_price",
            LocalPrice = new LocalPrice
            {
                Amount = product.PriceUsd.ToString("F2"),
                Currency = "USD"
            },
            Metadata = chargeMetadata,
            RedirectUrl = redirectUrl
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("charges", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Coinbase Commerce API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"Failed to create charge: {response.StatusCode}");
        }

        var chargeResponse = JsonSerializer.Deserialize<ChargeResponse>(responseBody, JsonOptions);
        if (chargeResponse?.Data == null)
        {
            throw new InvalidOperationException("Invalid response from Coinbase Commerce");
        }

        _logger.LogInformation(
            "Created Coinbase charge {ChargeCode} for user {UserId}, product {Product}",
            chargeResponse.Data.Code,
            userId,
            productType);

        return new CryptoCharge(
            ChargeId: chargeResponse.Data.Id,
            ChargeCode: chargeResponse.Data.Code,
            HostedUrl: chargeResponse.Data.HostedUrl,
            ExpiresAt: chargeResponse.Data.ExpiresAt
        );
    }

    public async Task<WebhookResult> ProcessWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default)
    {
        // Verify webhook signature
        if (!VerifyWebhookSignature(payload, signature))
        {
            _logger.LogWarning("Invalid Coinbase Commerce webhook signature");
            return new WebhookResult(false, "invalid", null, "Invalid webhook signature");
        }

        var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(payload, JsonOptions);
        if (webhookEvent?.Event == null)
        {
            return new WebhookResult(false, "unknown", null, "Could not parse webhook event");
        }

        var eventType = webhookEvent.Event.Type;
        _logger.LogInformation("Processing Coinbase Commerce webhook: {EventType}", eventType);

        try
        {
            switch (eventType)
            {
                case "charge:confirmed":
                case "charge:resolved":
                    await HandleChargeConfirmedAsync(webhookEvent.Event.Data, ct);
                    break;

                case "charge:failed":
                    await HandleChargeFailedAsync(webhookEvent.Event.Data, ct);
                    break;

                case "charge:pending":
                    _logger.LogInformation("Charge pending: {ChargeCode}", webhookEvent.Event.Data?.Code);
                    break;

                default:
                    _logger.LogDebug("Unhandled Coinbase event type: {EventType}", eventType);
                    break;
            }

            return new WebhookResult(true, eventType, webhookEvent.Event.Data?.Metadata?.GetValueOrDefault("user_id"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Coinbase webhook: {EventType}", eventType);
            return new WebhookResult(false, eventType, null, ex.Message);
        }
    }

    public Dictionary<string, decimal> GetPricing()
    {
        return _products.ToDictionary(p => p.Key, p => p.Value.PriceUsd);
    }

    private bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret))
        {
            _logger.LogWarning("Webhook secret not configured, skipping signature verification");
            return true;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

        return signature.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);
    }

    private async Task HandleChargeConfirmedAsync(ChargeData? chargeData, CancellationToken ct)
    {
        if (chargeData?.Metadata == null) return;

        var userId = chargeData.Metadata.GetValueOrDefault("user_id");
        var productType = chargeData.Metadata.GetValueOrDefault("product_type");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productType))
        {
            _logger.LogWarning("Charge confirmed but missing metadata: {ChargeCode}", chargeData.Code);
            return;
        }

        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null)
        {
            _logger.LogWarning("Charge confirmed for unknown user: {UserId}", userId);
            return;
        }

        switch (productType.ToLowerInvariant())
        {
            case "reveal":
                var walletAddress = chargeData.Metadata.GetValueOrDefault("wallet_address");
                if (!string.IsNullOrEmpty(walletAddress) &&
                    !user.RevealedWallets.Contains(walletAddress, StringComparer.OrdinalIgnoreCase))
                {
                    user.RevealedWallets.Add(walletAddress);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userService.UpdateUserAsync(user, ct);

                    _logger.LogInformation(
                        "Granted wallet reveal to user {UserId}: {Wallet} (crypto payment)",
                        userId, ShortenAddress(walletAddress));
                }
                break;

            case "lifetime":
                user.SubscriptionTier = "lifetime";
                user.SubscriptionExpiresAt = null; // Never expires
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user, ct);

                _logger.LogInformation("Granted lifetime membership to user {UserId} (crypto payment)", userId);
                break;

            default:
                _logger.LogWarning("Unknown product type in confirmed charge: {ProductType}", productType);
                break;
        }
    }

    private Task HandleChargeFailedAsync(ChargeData? chargeData, CancellationToken ct)
    {
        _logger.LogWarning(
            "Charge failed: {ChargeCode} for user {UserId}",
            chargeData?.Code,
            chargeData?.Metadata?.GetValueOrDefault("user_id"));

        return Task.CompletedTask;
    }

    private static string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }
}

/// <summary>
/// Configuration for Coinbase Commerce products
/// </summary>
public class CoinbaseProductConfig
{
    public decimal RevealPriceUsd { get; set; } = 5.00m;
    public decimal LifetimePriceUsd { get; set; } = 0m; // 0 = not available
}

// Internal models for Coinbase Commerce API
internal record CryptoProduct(string Name, string Description, decimal PriceUsd);

internal class CreateChargeRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PricingType { get; set; } = "fixed_price";
    public LocalPrice? LocalPrice { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? RedirectUrl { get; set; }
}

internal class LocalPrice
{
    public string Amount { get; set; } = "";
    public string Currency { get; set; } = "USD";
}

internal class ChargeResponse
{
    public ChargeData? Data { get; set; }
}

internal class ChargeData
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string HostedUrl { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

internal class WebhookEvent
{
    public WebhookEventData? Event { get; set; }
}

internal class WebhookEventData
{
    public string Type { get; set; } = "";
    public ChargeData? Data { get; set; }
}
