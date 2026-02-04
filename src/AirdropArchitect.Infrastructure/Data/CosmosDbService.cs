using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Data;

/// <summary>
/// Base class for Cosmos DB operations with resilience built-in
/// </summary>
public abstract class CosmosDbService<T> where T : class
{
    protected readonly Container Container;
    protected readonly ILogger Logger;

    protected CosmosDbService(
        CosmosClient cosmosClient,
        string databaseName,
        string containerName,
        ILogger logger)
    {
        Container = cosmosClient.GetContainer(databaseName, containerName);
        Logger = logger;
    }

    protected async Task<T?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken ct = default)
    {
        try
        {
            var response = await Container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: ct);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    protected async Task<T> UpsertAsync(
        T item,
        string partitionKey,
        CancellationToken ct = default)
    {
        var response = await Container.UpsertItemAsync(
            item,
            new PartitionKey(partitionKey),
            cancellationToken: ct);

        Logger.LogDebug(
            "Upserted item in {Container}, RU charge: {RuCharge}",
            Container.Id,
            response.RequestCharge);

        return response.Resource;
    }

    protected async Task<T> CreateAsync(
        T item,
        string partitionKey,
        CancellationToken ct = default)
    {
        var response = await Container.CreateItemAsync(
            item,
            new PartitionKey(partitionKey),
            cancellationToken: ct);

        Logger.LogDebug(
            "Created item in {Container}, RU charge: {RuCharge}",
            Container.Id,
            response.RequestCharge);

        return response.Resource;
    }

    protected async Task<T> ReplaceAsync(
        string id,
        T item,
        string partitionKey,
        CancellationToken ct = default)
    {
        var response = await Container.ReplaceItemAsync(
            item,
            id,
            new PartitionKey(partitionKey),
            cancellationToken: ct);

        Logger.LogDebug(
            "Replaced item {Id} in {Container}, RU charge: {RuCharge}",
            id,
            Container.Id,
            response.RequestCharge);

        return response.Resource;
    }

    protected async Task DeleteAsync(
        string id,
        string partitionKey,
        CancellationToken ct = default)
    {
        await Container.DeleteItemAsync<T>(
            id,
            new PartitionKey(partitionKey),
            cancellationToken: ct);

        Logger.LogDebug("Deleted item {Id} from {Container}", id, Container.Id);
    }

    protected async Task<List<T>> QueryAsync(
        string query,
        Dictionary<string, object>? parameters = null,
        CancellationToken ct = default)
    {
        var queryDefinition = new QueryDefinition(query);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                queryDefinition = queryDefinition.WithParameter(param.Key, param.Value);
            }
        }

        var results = new List<T>();
        var iterator = Container.GetItemQueryIterator<T>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(ct);

            Logger.LogDebug(
                "Query returned {Count} items, RU charge: {RuCharge}",
                response.Count,
                response.RequestCharge);

            results.AddRange(response);
        }

        return results;
    }

    protected async Task<T?> QuerySingleAsync(
        string query,
        Dictionary<string, object>? parameters = null,
        CancellationToken ct = default)
    {
        var results = await QueryAsync(query, parameters, ct);
        return results.FirstOrDefault();
    }
}
