using System.Text.Json;
using System.Text.Json.Serialization;
using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AirdropArchitect.Infrastructure.Points;

/// <summary>
/// Points provider for Ethena protocol (Sats).
/// Queries the Ethena API for sats balance.
/// </summary>
public class EthenaPointsProvider : IPointsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EthenaPointsProvider> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const string ApiBaseUrl = "https://app.ethena.fi/api/users/points";

    public string ProtocolName => "Ethena";

    public EthenaPointsProvider(ILogger<EthenaPointsProvider> logger)
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
        return protocolName.Equals("Ethena", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PointsData?> GetPointsAsync(string walletAddress, CancellationToken ct = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var url = $"{ApiBaseUrl}/{walletAddress.ToLowerInvariant()}";
                var response = await _httpClient.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Ethena API returned {StatusCode} for {Wallet}",
                        response.StatusCode, walletAddress[..10]);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(ct);
                var apiResponse = JsonSerializer.Deserialize<EthenaPointsResponse>(content);

                if (apiResponse == null)
                {
                    _logger.LogWarning("Could not parse Ethena response for {Wallet}", walletAddress[..10]);
                    return null;
                }

                var totalSats = apiResponse.TotalSats;

                _logger.LogDebug(
                    "Fetched Ethena sats for {Wallet}: {Sats}",
                    walletAddress[..10], totalSats);

                return new PointsData(
                    Points: totalSats,
                    Rank: apiResponse.Rank,
                    Percentile: null,
                    EstimatedValueUsd: null
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Ethena sats for {Wallet}", walletAddress[..10]);
            return null;
        }
    }
}

// Response models for Ethena API
internal class EthenaPointsResponse
{
    [JsonPropertyName("totalSats")]
    public decimal TotalSats { get; set; }

    [JsonPropertyName("rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("holdingSats")]
    public decimal? HoldingSats { get; set; }

    [JsonPropertyName("referralSats")]
    public decimal? ReferralSats { get; set; }
}
