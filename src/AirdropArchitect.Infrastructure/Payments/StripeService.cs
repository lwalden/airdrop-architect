using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace AirdropArchitect.Infrastructure.Payments;

/// <summary>
/// Stripe payment service implementation
/// </summary>
public class StripeService : IPaymentService
{
    private readonly IUserService _userService;
    private readonly ILogger<StripeService> _logger;
    private readonly string _webhookSecret;
    private readonly Dictionary<string, string> _tierToPriceId;
    private readonly string _revealPriceId;

    // Stripe event type constants
    private const string EventCheckoutSessionCompleted = "checkout.session.completed";
    private const string EventSubscriptionCreated = "customer.subscription.created";
    private const string EventSubscriptionUpdated = "customer.subscription.updated";
    private const string EventSubscriptionDeleted = "customer.subscription.deleted";
    private const string EventInvoicePaid = "invoice.paid";
    private const string EventInvoicePaymentFailed = "invoice.payment_failed";

    public StripeService(
        string secretKey,
        string webhookSecret,
        StripeProductConfig productConfig,
        IUserService userService,
        ILogger<StripeService> logger)
    {
        StripeConfiguration.ApiKey = secretKey;
        _webhookSecret = webhookSecret;
        _userService = userService;
        _logger = logger;

        _tierToPriceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tracker"] = productConfig.TrackerPriceId,
            ["architect"] = productConfig.ArchitectPriceId,
            ["api"] = productConfig.ApiPriceId
        };
        _revealPriceId = productConfig.RevealPriceId;
    }

    public async Task<CheckoutSession> CreateSubscriptionCheckoutAsync(
        string userId,
        string tier,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        if (!_tierToPriceId.TryGetValue(tier, out var priceId))
        {
            throw new ArgumentException($"Invalid subscription tier: {tier}. Valid tiers: tracker, architect, api");
        }

        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User not found: {userId}");
        }

        // Get or create Stripe customer
        var customerId = await GetOrCreateCustomerAsync(user, ct);

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["user_id"] = userId,
                ["tier"] = tier
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["user_id"] = userId,
                    ["tier"] = tier
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation(
            "Created subscription checkout session {SessionId} for user {UserId}, tier {Tier}",
            session.Id, userId, tier);

        return new CheckoutSession(
            SessionId: session.Id,
            Url: session.Url,
            ExpiresAt: session.ExpiresAt
        );
    }

    public async Task<CheckoutSession> CreateRevealCheckoutAsync(
        string userId,
        string walletAddress,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null)
        {
            throw new InvalidOperationException($"User not found: {userId}");
        }

        var customerId = await GetOrCreateCustomerAsync(user, ct);

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _revealPriceId,
                    Quantity = 1
                }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                ["user_id"] = userId,
                ["wallet_address"] = walletAddress,
                ["type"] = "reveal"
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        _logger.LogInformation(
            "Created reveal checkout session {SessionId} for user {UserId}, wallet {Wallet}",
            session.Id, userId, ShortenAddress(walletAddress));

        return new CheckoutSession(
            SessionId: session.Id,
            Url: session.Url,
            ExpiresAt: session.ExpiresAt
        );
    }

    public async Task<WebhookResult> ProcessWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct = default)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Invalid Stripe webhook signature: {Message}", ex.Message);
            return new WebhookResult(false, "invalid", null, "Invalid webhook signature");
        }

        _logger.LogInformation("Processing Stripe webhook event: {EventType}", stripeEvent.Type);

        try
        {
            switch (stripeEvent.Type)
            {
                case EventCheckoutSessionCompleted:
                    await HandleCheckoutCompletedAsync(stripeEvent, ct);
                    break;

                case EventSubscriptionCreated:
                case EventSubscriptionUpdated:
                    await HandleSubscriptionUpdatedAsync(stripeEvent, ct);
                    break;

                case EventSubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(stripeEvent, ct);
                    break;

                case EventInvoicePaid:
                    await HandleInvoicePaidAsync(stripeEvent, ct);
                    break;

                case EventInvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent, ct);
                    break;

                default:
                    _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return new WebhookResult(true, stripeEvent.Type, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook: {EventType}", stripeEvent.Type);
            return new WebhookResult(false, stripeEvent.Type, null, ex.Message);
        }
    }

    public async Task CancelSubscriptionAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userService.GetUserAsync(userId, ct);
        if (user?.StripeSubscriptionId == null)
        {
            throw new InvalidOperationException("User has no active subscription");
        }

        var service = new SubscriptionService();
        await service.UpdateAsync(
            user.StripeSubscriptionId,
            new SubscriptionUpdateOptions { CancelAtPeriodEnd = true },
            cancellationToken: ct);

        _logger.LogInformation(
            "Cancelled subscription {SubscriptionId} for user {UserId}",
            user.StripeSubscriptionId, userId);
    }

    public async Task<SubscriptionStatus?> GetSubscriptionStatusAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userService.GetUserAsync(userId, ct);
        if (user?.StripeSubscriptionId == null)
        {
            return null;
        }

        var service = new SubscriptionService();
        var subscription = await service.GetAsync(user.StripeSubscriptionId, cancellationToken: ct);

        return new SubscriptionStatus(
            UserId: userId,
            Tier: user.SubscriptionTier,
            Status: subscription.Status,
            CurrentPeriodStart: subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodStart,
            CurrentPeriodEnd: subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd,
            CancelAtPeriodEnd: subscription.CancelAtPeriodEnd,
            PaymentProvider: "stripe"
        );
    }

    public async Task<string> CreateCustomerPortalSessionAsync(
        string userId,
        string returnUrl,
        CancellationToken ct = default)
    {
        var user = await _userService.GetUserAsync(userId, ct);
        if (user?.StripeCustomerId == null)
        {
            throw new InvalidOperationException("User has no Stripe customer ID");
        }

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = user.StripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options, cancellationToken: ct);

        return session.Url;
    }

    private async Task<string> GetOrCreateCustomerAsync(Core.Models.User user, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
        {
            return user.StripeCustomerId;
        }

        var options = new CustomerCreateOptions
        {
            Email = user.Email,
            Metadata = new Dictionary<string, string>
            {
                ["user_id"] = user.Id,
                ["telegram_id"] = user.TelegramId?.ToString() ?? ""
            }
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options, cancellationToken: ct);

        user.StripeCustomerId = customer.Id;
        user.UpdatedAt = DateTime.UtcNow;
        await _userService.UpdateUserAsync(user, ct);

        _logger.LogInformation(
            "Created Stripe customer {CustomerId} for user {UserId}",
            customer.Id, user.Id);

        return customer.Id;
    }

    private async Task HandleCheckoutCompletedAsync(Event stripeEvent, CancellationToken ct)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null) return;

        var userId = session.Metadata.GetValueOrDefault("user_id");
        if (string.IsNullOrEmpty(userId)) return;

        // Handle one-time reveal purchase
        if (session.Metadata.GetValueOrDefault("type") == "reveal")
        {
            var walletAddress = session.Metadata.GetValueOrDefault("wallet_address");
            if (!string.IsNullOrEmpty(walletAddress))
            {
                await GrantWalletRevealAsync(userId, walletAddress, ct);
            }
        }

        _logger.LogInformation(
            "Checkout completed for user {UserId}, session {SessionId}",
            userId, session.Id);
    }

    private async Task HandleSubscriptionUpdatedAsync(Event stripeEvent, CancellationToken ct)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var userId = subscription.Metadata.GetValueOrDefault("user_id");
        var tier = subscription.Metadata.GetValueOrDefault("tier");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tier)) return;

        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null) return;

        user.StripeSubscriptionId = subscription.Id;
        user.SubscriptionTier = tier;
        // Get period end from first subscription item
        user.SubscriptionExpiresAt = subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;
        user.UpdatedAt = DateTime.UtcNow;

        await _userService.UpdateUserAsync(user, ct);

        _logger.LogInformation(
            "Updated subscription for user {UserId}: tier={Tier}, expires={Expires}",
            userId, tier, user.SubscriptionExpiresAt);
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent, CancellationToken ct)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var userId = subscription.Metadata.GetValueOrDefault("user_id");
        if (string.IsNullOrEmpty(userId)) return;

        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null) return;

        user.StripeSubscriptionId = null;
        user.SubscriptionTier = "free";
        user.SubscriptionExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userService.UpdateUserAsync(user, ct);

        _logger.LogInformation("Subscription deleted for user {UserId}", userId);
    }

    private Task HandleInvoicePaidAsync(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        _logger.LogInformation(
            "Invoice paid: {InvoiceId} for customer {CustomerId}",
            invoice?.Id, invoice?.CustomerId);
        return Task.CompletedTask;
    }

    private Task HandleInvoicePaymentFailedAsync(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        _logger.LogWarning(
            "Invoice payment failed: {InvoiceId} for customer {CustomerId}",
            invoice?.Id, invoice?.CustomerId);
        return Task.CompletedTask;
    }

    private async Task GrantWalletRevealAsync(string userId, string walletAddress, CancellationToken ct)
    {
        var user = await _userService.GetUserAsync(userId, ct);
        if (user == null) return;

        if (!user.RevealedWallets.Contains(walletAddress, StringComparer.OrdinalIgnoreCase))
        {
            user.RevealedWallets.Add(walletAddress);
            user.UpdatedAt = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user, ct);

            _logger.LogInformation(
                "Granted wallet reveal to user {UserId}: {Wallet}",
                userId, ShortenAddress(walletAddress));
        }
    }

    private static string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }
}

/// <summary>
/// Configuration for Stripe products and prices
/// </summary>
public class StripeProductConfig
{
    public string TrackerPriceId { get; set; } = "";
    public string ArchitectPriceId { get; set; } = "";
    public string ApiPriceId { get; set; } = "";
    public string RevealPriceId { get; set; } = "";
}
