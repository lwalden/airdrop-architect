namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for querying blockchain data across multiple EVM chains
/// </summary>
public interface IBlockchainService
{
    /// <summary>
    /// Get the native token balance (ETH, MATIC, etc.) for an address
    /// </summary>
    Task<WalletBalance> GetNativeBalanceAsync(string address, string chain = "ethereum", CancellationToken ct = default);

    /// <summary>
    /// Get ERC-20 token balances for an address using Alchemy's Token API
    /// </summary>
    Task<IReadOnlyList<TokenBalance>> GetTokenBalancesAsync(string address, string chain = "ethereum", CancellationToken ct = default);

    /// <summary>
    /// Get basic wallet activity metrics (transaction count, first/last activity)
    /// </summary>
    Task<WalletActivity> GetWalletActivityAsync(string address, string chain = "ethereum", CancellationToken ct = default);

    /// <summary>
    /// Check if an address is a contract or an EOA (Externally Owned Account)
    /// </summary>
    Task<bool> IsContractAsync(string address, string chain = "ethereum", CancellationToken ct = default);

    /// <summary>
    /// Get the current gas price on the network
    /// </summary>
    Task<GasPrice> GetGasPriceAsync(string chain = "ethereum", CancellationToken ct = default);

    /// <summary>
    /// Get the list of supported chains
    /// </summary>
    IReadOnlyList<string> GetSupportedChains();
}

/// <summary>
/// Native token balance result
/// </summary>
public record WalletBalance(
    string Address,
    string Chain,
    decimal BalanceInWei,
    decimal BalanceInEth,
    DateTime FetchedAt
);

/// <summary>
/// ERC-20 token balance
/// </summary>
public record TokenBalance(
    string ContractAddress,
    string? Symbol,
    string? Name,
    int Decimals,
    decimal RawBalance,
    decimal FormattedBalance
);

/// <summary>
/// Wallet activity metrics
/// </summary>
public record WalletActivity(
    string Address,
    string Chain,
    int TransactionCount,
    bool HasActivity,
    DateTime FetchedAt
);

/// <summary>
/// Current gas price information
/// </summary>
public record GasPrice(
    string Chain,
    decimal GasPriceWei,
    decimal GasPriceGwei,
    DateTime FetchedAt
);
