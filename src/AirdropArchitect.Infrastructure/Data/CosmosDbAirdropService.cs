using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Data;

/// <summary>
/// Cosmos DB implementation of airdrop service
/// </summary>
public class CosmosDbAirdropService : IAirdropService
{
    private readonly Container _airdropContainer;
    private readonly Container _eligibilityContainer;
    private readonly ILogger<CosmosDbAirdropService> _logger;

    private const string AirdropPartitionKey = "airdrops";

    public CosmosDbAirdropService(
        CosmosClient cosmosClient,
        string databaseName,
        ILogger<CosmosDbAirdropService> logger)
    {
        var database = cosmosClient.GetDatabase(databaseName);
        _airdropContainer = database.GetContainer("airdrops");
        _eligibilityContainer = database.GetContainer("eligibility");
        _logger = logger;
    }

    public async Task<List<Airdrop>> GetActiveAirdropsAsync(CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = @pk AND c.status IN ('claimable', 'upcoming')")
            .WithParameter("@pk", AirdropPartitionKey);

        var results = new List<Airdrop>();
        var iterator = _airdropContainer.GetItemQueryIterator<Airdrop>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Airdrop?> GetAirdropAsync(string airdropId, CancellationToken ct = default)
    {
        try
        {
            var response = await _airdropContainer.ReadItemAsync<Airdrop>(
                airdropId,
                new PartitionKey(AirdropPartitionKey),
                cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<EligibilityCheck>> CheckEligibilityAsync(
        string walletAddress,
        CancellationToken ct = default)
    {
        var airdrops = await GetActiveAirdropsAsync(ct);
        var results = new List<EligibilityCheck>();

        // Check each airdrop in parallel
        var tasks = airdrops.Select(a => CheckEligibilityForAirdropInternalAsync(walletAddress, a, ct));
        var checks = await Task.WhenAll(tasks);

        return checks.ToList();
    }

    public async Task<EligibilityCheck> CheckEligibilityForAirdropAsync(
        string walletAddress,
        string airdropId,
        CancellationToken ct = default)
    {
        var airdrop = await GetAirdropAsync(airdropId, ct);
        if (airdrop == null)
        {
            throw new KeyNotFoundException($"Airdrop {airdropId} not found");
        }

        return await CheckEligibilityForAirdropInternalAsync(walletAddress, airdrop, ct);
    }

    private async Task<EligibilityCheck> CheckEligibilityForAirdropInternalAsync(
        string walletAddress,
        Airdrop airdrop,
        CancellationToken ct)
    {
        // First check cache
        var cached = await GetCachedEligibilityForAirdropAsync(walletAddress, airdrop.Id, ct);

        if (cached != null && cached.CheckedAt > DateTime.UtcNow.AddHours(-24))
        {
            return new EligibilityCheck(
                AirdropId: airdrop.Id,
                AirdropName: airdrop.Name,
                TokenSymbol: airdrop.TokenSymbol,
                Status: airdrop.Status,
                IsEligible: cached.IsEligible,
                AllocationAmount: cached.AllocationAmount,
                AllocationUsd: cached.AllocationUsd,
                HasClaimed: cached.HasClaimed,
                ClaimDeadline: airdrop.ClaimDeadline,
                ClaimUrl: airdrop.ClaimUrl,
                Criteria: airdrop.Criteria
            );
        }

        // TODO: Implement actual eligibility checking based on checkMethod
        // For now, return unknown/not checked
        var result = new EligibilityCheck(
            AirdropId: airdrop.Id,
            AirdropName: airdrop.Name,
            TokenSymbol: airdrop.TokenSymbol,
            Status: airdrop.Status,
            IsEligible: false,
            AllocationAmount: null,
            AllocationUsd: null,
            HasClaimed: false,
            ClaimDeadline: airdrop.ClaimDeadline,
            ClaimUrl: airdrop.ClaimUrl,
            Criteria: airdrop.Criteria
        );

        _logger.LogDebug(
            "Checked eligibility for {Wallet} on {Airdrop}: {Eligible}",
            walletAddress[..10], airdrop.Name, result.IsEligible);

        return result;
    }

    private async Task<EligibilityResult?> GetCachedEligibilityForAirdropAsync(
        string walletAddress,
        string airdropId,
        CancellationToken ct)
    {
        var partitionKey = $"elig-{airdropId}";
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = @pk AND c.walletAddress = @wallet")
            .WithParameter("@pk", partitionKey)
            .WithParameter("@wallet", walletAddress.ToLowerInvariant());

        var iterator = _eligibilityContainer.GetItemQueryIterator<EligibilityResult>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<List<EligibilityResult>> GetCachedEligibilityAsync(
        string walletAddress,
        CancellationToken ct = default)
    {
        // Query across all eligibility partitions for this wallet
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.walletAddress = @wallet")
            .WithParameter("@wallet", walletAddress.ToLowerInvariant());

        var results = new List<EligibilityResult>();
        var iterator = _eligibilityContainer.GetItemQueryIterator<EligibilityResult>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Airdrop> UpsertAirdropAsync(Airdrop airdrop, CancellationToken ct = default)
    {
        airdrop.PartitionKey = AirdropPartitionKey;
        airdrop.UpdatedAt = DateTime.UtcNow;

        var response = await _airdropContainer.UpsertItemAsync(
            airdrop,
            new PartitionKey(AirdropPartitionKey),
            cancellationToken: ct);

        _logger.LogInformation("Upserted airdrop {AirdropId}: {Name}", airdrop.Id, airdrop.Name);

        return response.Resource;
    }
}
