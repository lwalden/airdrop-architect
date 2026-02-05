using AirdropArchitect.Core.Models;

namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for managing airdrops and checking eligibility
/// </summary>
public interface IAirdropService
{
    /// <summary>
    /// Get all active airdrops (claimable or upcoming)
    /// </summary>
    Task<List<Airdrop>> GetActiveAirdropsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific airdrop by ID
    /// </summary>
    Task<Airdrop?> GetAirdropAsync(string airdropId, CancellationToken ct = default);

    /// <summary>
    /// Check eligibility for a wallet across all active airdrops
    /// </summary>
    Task<List<EligibilityCheck>> CheckEligibilityAsync(
        string walletAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Check eligibility for a wallet for a specific airdrop
    /// </summary>
    Task<EligibilityCheck> CheckEligibilityForAirdropAsync(
        string walletAddress,
        string airdropId,
        CancellationToken ct = default);

    /// <summary>
    /// Get cached eligibility results for a wallet
    /// </summary>
    Task<List<EligibilityResult>> GetCachedEligibilityAsync(
        string walletAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Add or update an airdrop (admin function)
    /// </summary>
    Task<Airdrop> UpsertAirdropAsync(Airdrop airdrop, CancellationToken ct = default);
}

/// <summary>
/// Result of an eligibility check
/// </summary>
public record EligibilityCheck(
    string AirdropId,
    string AirdropName,
    string TokenSymbol,
    string Status,
    bool IsEligible,
    decimal? AllocationAmount,
    decimal? AllocationUsd,
    bool HasClaimed,
    DateTime? ClaimDeadline,
    string? ClaimUrl,
    List<string> Criteria
);
