using System.Text.Json.Serialization;

namespace AirdropArchitect.Core.Models;

/// <summary>
/// Represents a points/rewards program that may lead to an airdrop
/// </summary>
public class PointsProgram
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "points";

    [JsonPropertyName("protocolName")]
    public string ProtocolName { get; set; } = "";

    [JsonPropertyName("pointsName")]
    public string PointsName { get; set; } = "Points";

    [JsonPropertyName("chain")]
    public string Chain { get; set; } = "ethereum";

    /// <summary>
    /// Status: active, ended, upcoming
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";

    [JsonPropertyName("dashboardUrl")]
    public string? DashboardUrl { get; set; }

    /// <summary>
    /// API endpoint for fetching points (if available)
    /// </summary>
    [JsonPropertyName("apiEndpoint")]
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Method to track points: api, scrape, manual
    /// </summary>
    [JsonPropertyName("trackingMethod")]
    public string TrackingMethod { get; set; } = "manual";

    [JsonPropertyName("estimatedTgeDate")]
    public string? EstimatedTgeDate { get; set; }

    /// <summary>
    /// Token symbol once TGE occurs
    /// </summary>
    [JsonPropertyName("tokenSymbol")]
    public string? TokenSymbol { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Snapshot of a wallet's points at a point in time
/// </summary>
public class PointsSnapshot
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Partition key is "snapshot-{walletAddress}" for efficient queries per wallet
    /// </summary>
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "";

    [JsonPropertyName("walletAddress")]
    public string WalletAddress { get; set; } = "";

    [JsonPropertyName("programId")]
    public string ProgramId { get; set; } = "";

    [JsonPropertyName("protocolName")]
    public string ProtocolName { get; set; } = "";

    [JsonPropertyName("points")]
    public decimal Points { get; set; }

    [JsonPropertyName("rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("percentile")]
    public decimal? Percentile { get; set; }

    [JsonPropertyName("previousPoints")]
    public decimal? PreviousPoints { get; set; }

    [JsonPropertyName("pointsChange")]
    public decimal? PointsChange { get; set; }

    /// <summary>
    /// Estimated USD value based on comparable airdrops
    /// </summary>
    [JsonPropertyName("estimatedValueUsd")]
    public decimal? EstimatedValueUsd { get; set; }

    [JsonPropertyName("snapshotDate")]
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
}
