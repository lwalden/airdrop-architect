namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for handling payment operations (subscriptions and one-time purchases)
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a checkout session for a subscription upgrade
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="tier">Subscription tier: "tracker", "architect", or "api"</param>
    /// <param name="successUrl">URL to redirect after successful payment</param>
    /// <param name="cancelUrl">URL to redirect if payment cancelled</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Checkout session with URL for payment</returns>
    Task<CheckoutSession> CreateSubscriptionCheckoutAsync(
        string userId,
        string tier,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Create a checkout session for a one-time wallet reveal purchase
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="walletAddress">The wallet address being revealed</param>
    /// <param name="successUrl">URL to redirect after successful payment</param>
    /// <param name="cancelUrl">URL to redirect if payment cancelled</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Checkout session with URL for payment</returns>
    Task<CheckoutSession> CreateRevealCheckoutAsync(
        string userId,
        string walletAddress,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default);

    /// <summary>
    /// Process a webhook event from the payment provider
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
    /// Cancel a user's subscription
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="ct">Cancellation token</param>
    Task CancelSubscriptionAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Get the current subscription status for a user
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current subscription details or null if no subscription</returns>
    Task<SubscriptionStatus?> GetSubscriptionStatusAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Create or get a customer portal session for managing subscriptions
    /// </summary>
    /// <param name="userId">Internal user ID</param>
    /// <param name="returnUrl">URL to return to after portal session</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Portal session URL</returns>
    Task<string> CreateCustomerPortalSessionAsync(
        string userId,
        string returnUrl,
        CancellationToken ct = default);
}

/// <summary>
/// Checkout session result
/// </summary>
public record CheckoutSession(
    string SessionId,
    string Url,
    DateTime ExpiresAt
);

/// <summary>
/// Result of processing a webhook
/// </summary>
public record WebhookResult(
    bool Success,
    string EventType,
    string? UserId,
    string? ErrorMessage
);

/// <summary>
/// Current subscription status
/// </summary>
public record SubscriptionStatus(
    string UserId,
    string Tier,
    string Status,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    string? PaymentProvider
);
