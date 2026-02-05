namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for handling one-time cryptocurrency payments via Coinbase Commerce.
/// Recurring subscriptions use Stripe; this is for one-time purchases only.
/// </summary>
public interface ICryptoPaymentService
{
    /// <summary>
    /// Create a charge for a one-time purchase
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="productType">Product type (e.g., "reveal", "lifetime")</param>
    /// <param name="metadata">Additional metadata for the charge</param>
    /// <param name="redirectUrl">URL to redirect after payment</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Charge with hosted checkout URL</returns>
    Task<CryptoCharge> CreateChargeAsync(
        string userId,
        string productType,
        Dictionary<string, string>? metadata,
        string redirectUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Process a webhook event from Coinbase Commerce
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Webhook signature for verification</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of processing the webhook</returns>
    Task<WebhookResult> ProcessWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default);

    /// <summary>
    /// Get pricing info for available crypto products
    /// </summary>
    Dictionary<string, decimal> GetPricing();
}

/// <summary>
/// Coinbase Commerce charge result
/// </summary>
public record CryptoCharge(
    string ChargeId,
    string ChargeCode,
    string HostedUrl,
    DateTime ExpiresAt
);
