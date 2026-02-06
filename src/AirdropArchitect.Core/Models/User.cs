using System.Text.Json.Serialization;

namespace AirdropArchitect.Core.Models;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "users";

    [JsonPropertyName("telegramId")]
    public long? TelegramId { get; set; }

    [JsonPropertyName("telegramChatId")]
    public long? TelegramChatId { get; set; }

    [JsonPropertyName("telegramUsername")]
    public string? TelegramUsername { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("subscriptionTier")]
    public string SubscriptionTier { get; set; } = "free";

    [JsonPropertyName("subscriptionExpiresAt")]
    public DateTime? SubscriptionExpiresAt { get; set; }

    [JsonPropertyName("stripeCustomerId")]
    public string? StripeCustomerId { get; set; }

    [JsonPropertyName("stripeSubscriptionId")]
    public string? StripeSubscriptionId { get; set; }

    [JsonPropertyName("wallets")]
    public List<TrackedWallet> Wallets { get; set; } = new();

    [JsonPropertyName("revealedWallets")]
    public List<string> RevealedWallets { get; set; } = new();

    [JsonPropertyName("referralCode")]
    public string ReferralCode { get; set; } = GenerateReferralCode();

    [JsonPropertyName("referredBy")]
    public string? ReferredBy { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "GB", "DE").
    /// Used for OFAC compliance geo-restriction checks.
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    /// <summary>
    /// ISO 639-1 language code (e.g., "en", "es", "de").
    /// Used for localization of bot messages.
    /// </summary>
    [JsonPropertyName("preferredLanguage")]
    public string PreferredLanguage { get; set; } = "en";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private static string GenerateReferralCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "")
            .Replace("+", "")
            .Replace("=", "")
            [..8]
            .ToUpper();
    }
}

public class TrackedWallet
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("chain")]
    public string Chain { get; set; } = "ethereum";

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
