using System.Text.Json.Serialization;

namespace AirdropArchitect.Core.Models;

/// <summary>
/// Represents an airdrop campaign
/// </summary>
public class Airdrop
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "airdrops";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("tokenSymbol")]
    public string TokenSymbol { get; set; } = "";

    [JsonPropertyName("chain")]
    public string Chain { get; set; } = "ethereum";

    /// <summary>
    /// Status: upcoming, claimable, expired
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "upcoming";

    [JsonPropertyName("claimContract")]
    public string? ClaimContract { get; set; }

    [JsonPropertyName("claimUrl")]
    public string? ClaimUrl { get; set; }

    [JsonPropertyName("claimDeadline")]
    public DateTime? ClaimDeadline { get; set; }

    [JsonPropertyName("snapshotDate")]
    public DateTime? SnapshotDate { get; set; }

    /// <summary>
    /// URL or description of eligibility source (e.g., GitHub repo with merkle tree)
    /// </summary>
    [JsonPropertyName("eligibilitySource")]
    public string? EligibilitySource { get; set; }

    /// <summary>
    /// Human-readable eligibility criteria
    /// </summary>
    [JsonPropertyName("criteria")]
    public List<string> Criteria { get; set; } = new();

    [JsonPropertyName("totalEligibleAddresses")]
    public int? TotalEligibleAddresses { get; set; }

    [JsonPropertyName("averageAllocationUsd")]
    public decimal? AverageAllocationUsd { get; set; }

    /// <summary>
    /// Method to check eligibility: merkle, api, manual
    /// </summary>
    [JsonPropertyName("checkMethod")]
    public string CheckMethod { get; set; } = "manual";

    /// <summary>
    /// API endpoint for eligibility checking (if checkMethod is "api")
    /// </summary>
    [JsonPropertyName("eligibilityApiUrl")]
    public string? EligibilityApiUrl { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cached eligibility result for a wallet/airdrop combination
/// </summary>
public class EligibilityResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Partition key is "elig-{airdropId}" for efficient queries
    /// </summary>
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "";

    [JsonPropertyName("airdropId")]
    public string AirdropId { get; set; } = "";

    [JsonPropertyName("walletAddress")]
    public string WalletAddress { get; set; } = "";

    [JsonPropertyName("isEligible")]
    public bool IsEligible { get; set; }

    [JsonPropertyName("allocationAmount")]
    public decimal? AllocationAmount { get; set; }

    [JsonPropertyName("allocationUsd")]
    public decimal? AllocationUsd { get; set; }

    [JsonPropertyName("hasClaimed")]
    public bool HasClaimed { get; set; }

    /// <summary>
    /// Merkle proof if applicable
    /// </summary>
    [JsonPropertyName("merkleProof")]
    public List<string>? MerkleProof { get; set; }

    [JsonPropertyName("checkedAt")]
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
