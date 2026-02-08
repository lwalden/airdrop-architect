using AirdropArchitect.Core.Models;

namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Provider interface for checking wallet eligibility for a specific airdrop.
/// Implementations handle different check methods (api, merkle, manual).
/// </summary>
public interface IEligibilityChecker
{
    /// <summary>
    /// The check method this checker handles (e.g., "api", "merkle", "manual")
    /// </summary>
    string CheckMethod { get; }

    /// <summary>
    /// Whether this checker can handle the given check method
    /// </summary>
    bool CanHandle(string checkMethod);

    /// <summary>
    /// Check whether a wallet is eligible for a specific airdrop
    /// </summary>
    Task<EligibilityCheckResult> CheckAsync(
        string walletAddress,
        Airdrop airdrop,
        CancellationToken ct = default);
}

/// <summary>
/// Result from an eligibility checker
/// </summary>
public record EligibilityCheckResult(
    bool IsEligible,
    decimal? AllocationAmount,
    decimal? AllocationUsd,
    bool HasClaimed,
    List<string>? MerkleProof = null,
    string? ErrorMessage = null
);
