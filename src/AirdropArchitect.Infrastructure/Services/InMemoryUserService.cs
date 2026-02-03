using System.Collections.Concurrent;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Services;

/// <summary>
/// In-memory user service for development/testing.
/// Will be replaced with CosmosDbUserService in Week 4.
/// </summary>
public class InMemoryUserService : IUserService
{
    private readonly ConcurrentDictionary<string, User> _users = new();
    private readonly ConcurrentDictionary<long, string> _telegramIdToUserId = new();
    private readonly ILogger<InMemoryUserService> _logger;

    public InMemoryUserService(ILogger<InMemoryUserService> logger)
    {
        _logger = logger;
    }

    public Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        if (_telegramIdToUserId.TryGetValue(telegramId, out var userId) &&
            _users.TryGetValue(userId, out var user))
        {
            return Task.FromResult(user);
        }

        throw new KeyNotFoundException($"User with Telegram ID {telegramId} not found");
    }

    public Task<User> EnsureUserExistsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        if (_telegramIdToUserId.TryGetValue(telegramId, out var existingUserId) &&
            _users.TryGetValue(existingUserId, out var existingUser))
        {
            return Task.FromResult(existingUser);
        }

        var user = new User
        {
            TelegramId = telegramId,
            TelegramChatId = telegramId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _users[user.Id] = user;
        _telegramIdToUserId[telegramId] = user.Id;

        _logger.LogInformation("Created new user {UserId} for Telegram ID {TelegramId}", user.Id, telegramId);

        return Task.FromResult(user);
    }

    public Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _users[user.Id] = user;
        return Task.FromResult(user);
    }

    public Task AddWalletAsync(string userId, TrackedWallet wallet, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            throw new KeyNotFoundException($"User {userId} not found");
        }

        user.Wallets.Add(wallet);
        user.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Added wallet {Address} to user {UserId}", wallet.Address, userId);

        return Task.CompletedTask;
    }

    public Task RemoveWalletAsync(string userId, string walletAddress, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            throw new KeyNotFoundException($"User {userId} not found");
        }

        var wallet = user.Wallets.FirstOrDefault(w =>
            w.Address.Equals(walletAddress, StringComparison.OrdinalIgnoreCase));

        if (wallet != null)
        {
            user.Wallets.Remove(wallet);
            user.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Removed wallet {Address} from user {UserId}", walletAddress, userId);
        }

        return Task.CompletedTask;
    }
}
