namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Interface for protocol-specific points providers.
/// Each implementation fetches points data from a specific protocol's API.
/// </summary>
public interface IPointsProvider
{
    string ProtocolName { get; }
    bool CanHandle(string protocolName);
    Task<PointsData?> GetPointsAsync(string walletAddress, CancellationToken ct = default);
}

/// <summary>
/// Raw points data returned by a provider
/// </summary>
public record PointsData(
    decimal Points,
    int? Rank,
    decimal? Percentile,
    decimal? EstimatedValueUsd
);
