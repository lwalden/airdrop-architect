using System.Text.Json;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AirdropArchitect.Infrastructure.Eligibility;

/// <summary>
/// Checks airdrop eligibility by calling the airdrop's API endpoint.
/// Handles checkMethod == "api".
/// Requires Airdrop.EligibilityApiUrl to be set.
/// </summary>
public class ApiEligibilityChecker : IEligibilityChecker
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiEligibilityChecker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public string CheckMethod => "api";

    public ApiEligibilityChecker(ILogger<ApiEligibilityChecker> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AirdropArchitect/1.0");

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }

    public bool CanHandle(string checkMethod)
    {
        return checkMethod.Equals("api", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<EligibilityCheckResult> CheckAsync(
        string walletAddress,
        Airdrop airdrop,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(airdrop.EligibilityApiUrl))
        {
            _logger.LogWarning(
                "Airdrop {AirdropId} has checkMethod=api but no EligibilityApiUrl",
                airdrop.Id);
            return new EligibilityCheckResult(
                IsEligible: false,
                AllocationAmount: null,
                AllocationUsd: null,
                HasClaimed: false,
                ErrorMessage: "Eligibility API not configured for this airdrop");
        }

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var url = BuildUrl(airdrop.EligibilityApiUrl, walletAddress);
                _logger.LogDebug("Checking eligibility for {Wallet} at {Url}",
                    walletAddress[..10], url);

                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Eligibility API returned {StatusCode} for {Airdrop} / {Wallet}",
                        response.StatusCode, airdrop.Name, walletAddress[..10]);

                    return new EligibilityCheckResult(
                        IsEligible: false,
                        AllocationAmount: null,
                        AllocationUsd: null,
                        HasClaimed: false,
                        ErrorMessage: $"API returned {(int)response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                return ParseResponse(content, airdrop);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking eligibility for {Wallet} on {Airdrop}",
                walletAddress[..10], airdrop.Name);

            return new EligibilityCheckResult(
                IsEligible: false,
                AllocationAmount: null,
                AllocationUsd: null,
                HasClaimed: false,
                ErrorMessage: "Error calling eligibility API");
        }
    }

    private static string BuildUrl(string apiUrl, string walletAddress)
    {
        var separator = apiUrl.Contains('?') ? "&" : "?";
        return $"{apiUrl}{separator}address={walletAddress}";
    }

    /// <summary>
    /// Parses common eligibility API response patterns.
    /// Different protocols use different JSON structures, so we try multiple field names.
    /// </summary>
    private EligibilityCheckResult ParseResponse(string json, Airdrop airdrop)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check for eligible/isEligible boolean field
            var isEligible = TryGetBool(root, "eligible")
                ?? TryGetBool(root, "isEligible")
                ?? TryGetBool(root, "is_eligible");

            // Check for allocation amount
            var amount = TryGetDecimal(root, "amount")
                ?? TryGetDecimal(root, "allocation")
                ?? TryGetDecimal(root, "allocationAmount")
                ?? TryGetDecimal(root, "token_amount");

            // Check for USD value
            var usdValue = TryGetDecimal(root, "amountUsd")
                ?? TryGetDecimal(root, "usd_value")
                ?? TryGetDecimal(root, "allocationUsd");

            // Check for claimed status
            var hasClaimed = TryGetBool(root, "claimed")
                ?? TryGetBool(root, "hasClaimed")
                ?? TryGetBool(root, "isClaimed")
                ?? false;

            // If no explicit eligible field, infer from allocation > 0
            if (isEligible == null && amount != null)
            {
                isEligible = amount > 0;
            }

            // If still no eligible field, check for presence of data
            // Some APIs return 200 with data if eligible, or empty/error if not
            if (isEligible == null)
            {
                isEligible = amount != null && amount > 0;
            }

            _logger.LogDebug(
                "Parsed eligibility for {Airdrop}: eligible={Eligible}, amount={Amount}",
                airdrop.Name, isEligible, amount);

            return new EligibilityCheckResult(
                IsEligible: isEligible ?? false,
                AllocationAmount: amount,
                AllocationUsd: usdValue,
                HasClaimed: hasClaimed);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "Could not parse eligibility response for {Airdrop}", airdrop.Name);

            return new EligibilityCheckResult(
                IsEligible: false,
                AllocationAmount: null,
                AllocationUsd: null,
                HasClaimed: false,
                ErrorMessage: "Could not parse API response");
        }
    }

    private static bool? TryGetBool(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.True) return true;
            if (prop.ValueKind == JsonValueKind.False) return false;
            if (prop.ValueKind == JsonValueKind.String)
            {
                var str = prop.GetString();
                if (bool.TryParse(str, out var b)) return b;
            }
        }
        return null;
    }

    private static decimal? TryGetDecimal(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var d))
                return d;
            if (prop.ValueKind == JsonValueKind.String)
            {
                var str = prop.GetString();
                if (decimal.TryParse(str, out var d2)) return d2;
            }
        }
        return null;
    }
}
