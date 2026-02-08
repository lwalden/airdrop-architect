using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AirdropArchitect.Infrastructure.Points;

/// <summary>
/// Points provider for EigenLayer restaking protocol.
/// Queries the EigenLayer staker API for restaked points.
/// </summary>
public class EigenLayerPointsProvider : IPointsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EigenLayerPointsProvider> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const string ApiBaseUrl = "https://claims.eigenfoundation.org/clique-eigenlayer-api/campaign/eigenlayer/credentials";

    public string ProtocolName => "EigenLayer";

    public EigenLayerPointsProvider(ILogger<EigenLayerPointsProvider> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }

    public bool CanHandle(string protocolName)
    {
        return protocolName.Equals("EigenLayer", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PointsData?> GetPointsAsync(string walletAddress, CancellationToken ct = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var url = $"{ApiBaseUrl}?walletAddress={walletAddress.ToLowerInvariant()}";
                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "EigenLayer API returned {StatusCode} for {Wallet}",
                        response.StatusCode, walletAddress[..10]);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                var apiResponse = JsonSerializer.Deserialize<EigenLayerResponse>(content);

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning("Could not parse EigenLayer response for {Wallet}", walletAddress[..10]);
                    return null;
                }

                var totalPoints = apiResponse.Data.EigenLayerPoints;

                _logger.LogDebug(
                    "Fetched EigenLayer points for {Wallet}: {Points}",
                    walletAddress[..10], totalPoints);

                return new PointsData(
                    Points: totalPoints,
                    Rank: null,
                    Percentile: null,
                    EstimatedValueUsd: null
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching EigenLayer points for {Wallet}", walletAddress[..10]);
            return null;
        }
    }
}

// Response models for EigenLayer API
internal class EigenLayerResponse
{
    [JsonPropertyName("data")]
    public EigenLayerData? Data { get; set; }
}

internal class EigenLayerData
{
    [JsonPropertyName("eigenLayerPoints")]
    public decimal EigenLayerPoints { get; set; }

    [JsonPropertyName("beaconChainETHRestaked")]
    public decimal? BeaconChainEthRestaked { get; set; }

    [JsonPropertyName("lrtPoints")]
    public decimal? LrtPoints { get; set; }
}
