using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AirdropArchitect.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AirdropArchitect.Infrastructure.Points;

/// <summary>
/// Points provider for Hyperliquid protocol
/// API docs: https://hyperliquid.gitbook.io/hyperliquid-docs/for-developers/api
/// </summary>
public class HyperliquidPointsProvider : IPointsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HyperliquidPointsProvider> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const string ApiBaseUrl = "https://api.hyperliquid.xyz/info";

    public string ProtocolName => "Hyperliquid";

    public HyperliquidPointsProvider(ILogger<HyperliquidPointsProvider> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }

    public bool CanHandle(string protocolName)
    {
        return protocolName.Equals("Hyperliquid", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PointsData?> GetPointsAsync(string walletAddress, CancellationToken ct = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                // Hyperliquid API uses POST with JSON body
                var request = new HyperliquidPointsRequest
                {
                    Type = "userPoints",
                    User = walletAddress.ToLowerInvariant()
                };

                var response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Hyperliquid API returned {StatusCode} for {Wallet}",
                        response.StatusCode, walletAddress[..10]);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(ct);

                // Try to parse the response
                var pointsResponse = JsonSerializer.Deserialize<HyperliquidPointsResponse>(content);

                if (pointsResponse == null)
                {
                    _logger.LogWarning("Could not parse Hyperliquid response for {Wallet}", walletAddress[..10]);
                    return null;
                }

                // Calculate total points from all sources
                var totalPoints = pointsResponse.TotalPoints;

                _logger.LogDebug(
                    "Fetched Hyperliquid points for {Wallet}: {Points}",
                    walletAddress[..10], totalPoints);

                return new PointsData(
                    Points: totalPoints,
                    Rank: pointsResponse.Rank,
                    Percentile: null, // Not provided by API
                    EstimatedValueUsd: null // Would need token price data
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Hyperliquid points for {Wallet}", walletAddress[..10]);
            return null;
        }
    }
}

// Request/Response models for Hyperliquid API
internal class HyperliquidPointsRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("user")]
    public string User { get; set; } = "";
}

internal class HyperliquidPointsResponse
{
    [JsonPropertyName("totalPoints")]
    public decimal TotalPoints { get; set; }

    [JsonPropertyName("rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("referralPoints")]
    public decimal? ReferralPoints { get; set; }

    [JsonPropertyName("tradingPoints")]
    public decimal? TradingPoints { get; set; }

    [JsonPropertyName("stakingPoints")]
    public decimal? StakingPoints { get; set; }
}
