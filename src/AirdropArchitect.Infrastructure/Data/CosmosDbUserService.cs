using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = AirdropArchitect.Core.Models.User;

namespace AirdropArchitect.Infrastructure.Data;

/// <summary>
/// Cosmos DB implementation of user service for persistent storage
/// </summary>
public class CosmosDbUserService : CosmosDbService<User>, IUserService
{
    private const string DefaultPartitionKey = "users";

    public CosmosDbUserService(
        CosmosClient cosmosClient,
        string databaseName,
        ILogger<CosmosDbUserService> logger)
        : base(cosmosClient, databaseName, "users", logger)
    {
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(userId, DefaultPartitionKey, cancellationToken);
    }

    public async Task<User> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM c WHERE c.telegramId = @telegramId";
        var parameters = new Dictionary<string, object>
        {
            ["@telegramId"] = telegramId
        };

        var user = await QuerySingleAsync(query, parameters, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with Telegram ID {telegramId} not found");
        }

        return user;
    }

    public async Task<User> EnsureUserExistsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        // Try to find existing user
        var query = "SELECT * FROM c WHERE c.telegramId = @telegramId";
        var parameters = new Dictionary<string, object>
        {
            ["@telegramId"] = telegramId
        };

        var existingUser = await QuerySingleAsync(query, parameters, cancellationToken);

        if (existingUser != null)
        {
            return existingUser;
        }

        // Create new user
        var user = new User
        {
            TelegramId = telegramId,
            TelegramChatId = telegramId,
            PartitionKey = DefaultPartitionKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await CreateAsync(user, DefaultPartitionKey, cancellationToken);

        Logger.LogInformation(
            "Created new user {UserId} for Telegram ID {TelegramId}",
            createdUser.Id,
            telegramId);

        return createdUser;
    }

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        return await ReplaceAsync(user.Id, user, user.PartitionKey, cancellationToken);
    }

    public async Task AddWalletAsync(string userId, TrackedWallet wallet, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User {userId} not found");
        }

        // Check for duplicate
        if (user.Wallets.Any(w => w.Address.Equals(wallet.Address, StringComparison.OrdinalIgnoreCase)))
        {
            Logger.LogDebug("Wallet {Address} already tracked for user {UserId}", wallet.Address, userId);
            return;
        }

        user.Wallets.Add(wallet);
        user.UpdatedAt = DateTime.UtcNow;

        await ReplaceAsync(user.Id, user, user.PartitionKey, cancellationToken);

        Logger.LogInformation("Added wallet {Address} to user {UserId}", wallet.Address, userId);
    }

    public async Task RemoveWalletAsync(string userId, string walletAddress, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User {userId} not found");
        }

        var wallet = user.Wallets.FirstOrDefault(w =>
            w.Address.Equals(walletAddress, StringComparison.OrdinalIgnoreCase));

        if (wallet != null)
        {
            user.Wallets.Remove(wallet);
            user.UpdatedAt = DateTime.UtcNow;

            await ReplaceAsync(user.Id, user, user.PartitionKey, cancellationToken);

            Logger.LogInformation("Removed wallet {Address} from user {UserId}", walletAddress, userId);
        }
    }
}
