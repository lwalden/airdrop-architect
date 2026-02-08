using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Data;

/// <summary>
/// Cosmos DB implementation of points service
/// </summary>
public class CosmosDbPointsService : IPointsService
{
    private readonly Container _programsContainer;
    private readonly Container _snapshotsContainer;
    private readonly IPointsProvider[] _pointsProviders;
    private readonly ILogger<CosmosDbPointsService> _logger;

    private const string ProgramsPartitionKey = "points";

    public CosmosDbPointsService(
        CosmosClient cosmosClient,
        string databaseName,
        IEnumerable<IPointsProvider> pointsProviders,
        ILogger<CosmosDbPointsService> logger)
    {
        var database = cosmosClient.GetDatabase(databaseName);
        _programsContainer = database.GetContainer("points");
        _snapshotsContainer = database.GetContainer("snapshots");
        _pointsProviders = pointsProviders.ToArray();
        _logger = logger;
    }

    public async Task<List<PointsProgram>> GetActiveProgramsAsync(CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = @pk AND c.status = 'active'")
            .WithParameter("@pk", ProgramsPartitionKey);

        var results = new List<PointsProgram>();
        var iterator = _programsContainer.GetItemQueryIterator<PointsProgram>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<PointsProgram?> GetProgramAsync(string programId, CancellationToken ct = default)
    {
        try
        {
            var response = await _programsContainer.ReadItemAsync<PointsProgram>(
                programId,
                new PartitionKey(ProgramsPartitionKey),
                cancellationToken: ct);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<PointsBalance>> GetPointsForWalletAsync(
        string walletAddress,
        CancellationToken ct = default)
    {
        var programs = await GetActiveProgramsAsync(ct);
        var results = new List<PointsBalance>();

        foreach (var program in programs)
        {
            var balance = await GetPointsForProgramAsync(walletAddress, program.Id, ct);
            if (balance != null)
            {
                results.Add(balance);
            }
        }

        return results;
    }

    public async Task<PointsBalance?> GetPointsForProgramAsync(
        string walletAddress,
        string programId,
        CancellationToken ct = default)
    {
        var program = await GetProgramAsync(programId, ct);
        if (program == null) return null;

        // Get latest snapshot
        var snapshot = await GetLatestSnapshotAsync(walletAddress, programId, ct);

        if (snapshot != null)
        {
            return new PointsBalance(
                ProgramId: program.Id,
                ProtocolName: program.ProtocolName,
                PointsName: program.PointsName,
                Points: snapshot.Points,
                Rank: snapshot.Rank,
                Percentile: snapshot.Percentile,
                EstimatedValueUsd: snapshot.EstimatedValueUsd,
                PointsChange24h: snapshot.PointsChange,
                DashboardUrl: program.DashboardUrl,
                LastUpdated: snapshot.SnapshotDate
            );
        }

        return null;
    }

    public async Task<List<PointsBalance>> RefreshPointsAsync(
        string walletAddress,
        CancellationToken ct = default)
    {
        var programs = await GetActiveProgramsAsync(ct);
        var results = new List<PointsBalance>();

        foreach (var program in programs)
        {
            try
            {
                // Find a provider that can handle this program
                var provider = _pointsProviders.FirstOrDefault(p => p.CanHandle(program.ProtocolName));

                if (provider != null)
                {
                    var pointsData = await provider.GetPointsAsync(walletAddress, ct);

                    if (pointsData != null)
                    {
                        // Get previous snapshot for change calculation
                        var previousSnapshot = await GetLatestSnapshotAsync(walletAddress, program.Id, ct);

                        // Create new snapshot
                        var snapshot = new PointsSnapshot
                        {
                            PartitionKey = $"snapshot-{walletAddress.ToLowerInvariant()}",
                            WalletAddress = walletAddress.ToLowerInvariant(),
                            ProgramId = program.Id,
                            ProtocolName = program.ProtocolName,
                            Points = pointsData.Points,
                            Rank = pointsData.Rank,
                            Percentile = pointsData.Percentile,
                            EstimatedValueUsd = pointsData.EstimatedValueUsd,
                            PreviousPoints = previousSnapshot?.Points,
                            PointsChange = previousSnapshot != null
                                ? pointsData.Points - previousSnapshot.Points
                                : null,
                            SnapshotDate = DateTime.UtcNow
                        };

                        await SaveSnapshotAsync(snapshot, ct);

                        results.Add(new PointsBalance(
                            ProgramId: program.Id,
                            ProtocolName: program.ProtocolName,
                            PointsName: program.PointsName,
                            Points: snapshot.Points,
                            Rank: snapshot.Rank,
                            Percentile: snapshot.Percentile,
                            EstimatedValueUsd: snapshot.EstimatedValueUsd,
                            PointsChange24h: snapshot.PointsChange,
                            DashboardUrl: program.DashboardUrl,
                            LastUpdated: snapshot.SnapshotDate
                        ));

                        _logger.LogInformation(
                            "Refreshed {Protocol} points for {Wallet}: {Points}",
                            program.ProtocolName, walletAddress[..10], pointsData.Points);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh points for {Protocol}", program.ProtocolName);
            }
        }

        return results;
    }

    private async Task<PointsSnapshot?> GetLatestSnapshotAsync(
        string walletAddress,
        string programId,
        CancellationToken ct)
    {
        var partitionKey = $"snapshot-{walletAddress.ToLowerInvariant()}";
        var query = new QueryDefinition(
            "SELECT TOP 1 * FROM c WHERE c.partitionKey = @pk AND c.programId = @programId ORDER BY c.snapshotDate DESC")
            .WithParameter("@pk", partitionKey)
            .WithParameter("@programId", programId);

        var iterator = _snapshotsContainer.GetItemQueryIterator<PointsSnapshot>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            return response.FirstOrDefault();
        }

        return null;
    }

    private async Task SaveSnapshotAsync(PointsSnapshot snapshot, CancellationToken ct)
    {
        await _snapshotsContainer.UpsertItemAsync(
            snapshot,
            new PartitionKey(snapshot.PartitionKey),
            cancellationToken: ct);
    }

    public async Task<List<PointsSnapshot>> GetSnapshotHistoryAsync(
        string walletAddress,
        string? programId = null,
        int limit = 30,
        CancellationToken ct = default)
    {
        var partitionKey = $"snapshot-{walletAddress.ToLowerInvariant()}";

        QueryDefinition query;
        if (programId != null)
        {
            query = new QueryDefinition(
                $"SELECT TOP {limit} * FROM c WHERE c.partitionKey = @pk AND c.programId = @programId ORDER BY c.snapshotDate DESC")
                .WithParameter("@pk", partitionKey)
                .WithParameter("@programId", programId);
        }
        else
        {
            query = new QueryDefinition(
                $"SELECT TOP {limit} * FROM c WHERE c.partitionKey = @pk ORDER BY c.snapshotDate DESC")
                .WithParameter("@pk", partitionKey);
        }

        var results = new List<PointsSnapshot>();
        var iterator = _snapshotsContainer.GetItemQueryIterator<PointsSnapshot>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<PointsProgram> UpsertProgramAsync(PointsProgram program, CancellationToken ct = default)
    {
        program.PartitionKey = ProgramsPartitionKey;
        program.LastUpdated = DateTime.UtcNow;

        var response = await _programsContainer.UpsertItemAsync(
            program,
            new PartitionKey(ProgramsPartitionKey),
            cancellationToken: ct);

        _logger.LogInformation("Upserted points program {ProgramId}: {Name}", program.Id, program.ProtocolName);

        return response.Resource;
    }
}
