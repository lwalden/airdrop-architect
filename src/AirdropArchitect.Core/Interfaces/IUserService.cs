using AirdropArchitect.Core.Models;

namespace AirdropArchitect.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserAsync(string oderId, CancellationToken cancellationToken = default);
    Task<User> GetUserByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User> EnsureUserExistsAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task AddWalletAsync(string userId, TrackedWallet wallet, CancellationToken cancellationToken = default);
    Task RemoveWalletAsync(string userId, string walletAddress, CancellationToken cancellationToken = default);
}
