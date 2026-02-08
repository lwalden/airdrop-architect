using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Eligibility;

/// <summary>
/// Fallback eligibility checker for airdrops that require manual verification.
/// Handles checkMethod == "manual" and "merkle" (merkle trees require large datasets
/// that aren't feasible to verify in real-time; users should check the claim URL directly).
/// </summary>
public class ManualEligibilityChecker : IEligibilityChecker
{
    private readonly ILogger<ManualEligibilityChecker> _logger;

    public string CheckMethod => "manual";

    public ManualEligibilityChecker(ILogger<ManualEligibilityChecker> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(string checkMethod)
    {
        return checkMethod.Equals("manual", StringComparison.OrdinalIgnoreCase)
            || checkMethod.Equals("merkle", StringComparison.OrdinalIgnoreCase);
    }

    public Task<EligibilityCheckResult> CheckAsync(
        string walletAddress,
        Airdrop airdrop,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Manual/merkle eligibility check for {Wallet} on {Airdrop} — cannot automate",
            walletAddress[..10], airdrop.Name);

        // We can't determine eligibility automatically for manual/merkle airdrops.
        // Return a result that indicates the check couldn't be automated,
        // so the display layer can show the claim URL and criteria instead.
        var result = new EligibilityCheckResult(
            IsEligible: false,
            AllocationAmount: null,
            AllocationUsd: null,
            HasClaimed: false,
            ErrorMessage: airdrop.CheckMethod == "merkle"
                ? "Check the claim page directly to verify eligibility"
                : "Manual verification required");

        return Task.FromResult(result);
    }
}
