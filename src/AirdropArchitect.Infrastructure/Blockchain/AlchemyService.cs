using System.Numerics;
using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Polly;
using Polly.Retry;

namespace AirdropArchitect.Infrastructure.Blockchain;

/// <summary>
/// Blockchain service implementation using Alchemy's RPC endpoints via Nethereum
/// </summary>
public class AlchemyService : IBlockchainService
{
    private readonly string _apiKey;
    private readonly ILogger<AlchemyService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly Dictionary<string, Web3> _web3Clients = new();

    private static readonly Dictionary<string, string> ChainEndpoints = new()
    {
        ["ethereum"] = "https://eth-mainnet.g.alchemy.com/v2/",
        ["arbitrum"] = "https://arb-mainnet.g.alchemy.com/v2/",
        ["optimism"] = "https://opt-mainnet.g.alchemy.com/v2/",
        ["base"] = "https://base-mainnet.g.alchemy.com/v2/",
        ["polygon"] = "https://polygon-mainnet.g.alchemy.com/v2/"
    };

    private static readonly Dictionary<string, string> ChainNativeSymbols = new()
    {
        ["ethereum"] = "ETH",
        ["arbitrum"] = "ETH",
        ["optimism"] = "ETH",
        ["base"] = "ETH",
        ["polygon"] = "MATIC"
    };

    public AlchemyService(string apiKey, ILogger<AlchemyService> logger)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure retry policy with exponential backoff
        // Blockchain RPCs can be flaky, so we retry with increasing delays
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} for blockchain call after {Delay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    public IReadOnlyList<string> GetSupportedChains() => ChainEndpoints.Keys.ToList();

    public async Task<WalletBalance> GetNativeBalanceAsync(
        string address,
        string chain = "ethereum",
        CancellationToken ct = default)
    {
        ValidateAddress(address);
        var web3 = GetWeb3Client(chain);

        var balanceWei = await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Fetching native balance for {Address} on {Chain}", address, chain);
            return await web3.Eth.GetBalance.SendRequestAsync(address);
        });

        var balanceEth = Web3.Convert.FromWei(balanceWei);

        _logger.LogInformation(
            "Retrieved balance for {Address} on {Chain}: {Balance} {Symbol}",
            ShortenAddress(address),
            chain,
            balanceEth,
            ChainNativeSymbols.GetValueOrDefault(chain, "ETH"));

        return new WalletBalance(
            Address: address,
            Chain: chain,
            BalanceInWei: (decimal)balanceWei.Value,
            BalanceInEth: balanceEth,
            FetchedAt: DateTime.UtcNow
        );
    }

    public async Task<IReadOnlyList<TokenBalance>> GetTokenBalancesAsync(
        string address,
        string chain = "ethereum",
        CancellationToken ct = default)
    {
        ValidateAddress(address);
        var web3 = GetWeb3Client(chain);

        // Alchemy has a special Token API, but for now we'll return an empty list
        // as getting all token balances requires the Alchemy Enhanced APIs
        // which use a different endpoint pattern (alchemy_getTokenBalances)

        // TODO: Implement Alchemy Token API integration for comprehensive token balances
        // For now, this is a placeholder that returns an empty list
        _logger.LogDebug(
            "Token balance fetching not yet implemented for {Address} on {Chain}",
            address,
            chain);

        return Array.Empty<TokenBalance>();
    }

    public async Task<WalletActivity> GetWalletActivityAsync(
        string address,
        string chain = "ethereum",
        CancellationToken ct = default)
    {
        ValidateAddress(address);
        var web3 = GetWeb3Client(chain);

        var txCount = await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Fetching transaction count for {Address} on {Chain}", address, chain);
            return await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);
        });

        var hasActivity = txCount.Value > 0;

        _logger.LogInformation(
            "Retrieved activity for {Address} on {Chain}: {TxCount} transactions",
            ShortenAddress(address),
            chain,
            txCount.Value);

        return new WalletActivity(
            Address: address,
            Chain: chain,
            TransactionCount: (int)txCount.Value,
            HasActivity: hasActivity,
            FetchedAt: DateTime.UtcNow
        );
    }

    public async Task<bool> IsContractAsync(
        string address,
        string chain = "ethereum",
        CancellationToken ct = default)
    {
        ValidateAddress(address);
        var web3 = GetWeb3Client(chain);

        var code = await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Checking if {Address} is a contract on {Chain}", address, chain);
            return await web3.Eth.GetCode.SendRequestAsync(address);
        });

        // If code is "0x" or empty, it's an EOA; otherwise it's a contract
        var isContract = !string.IsNullOrEmpty(code) && code != "0x";

        _logger.LogDebug(
            "{Address} on {Chain} is {Type}",
            ShortenAddress(address),
            chain,
            isContract ? "a contract" : "an EOA");

        return isContract;
    }

    public async Task<GasPrice> GetGasPriceAsync(
        string chain = "ethereum",
        CancellationToken ct = default)
    {
        var web3 = GetWeb3Client(chain);

        var gasPriceWei = await _retryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Fetching gas price on {Chain}", chain);
            return await web3.Eth.GasPrice.SendRequestAsync();
        });

        var gasPriceGwei = Web3.Convert.FromWei(gasPriceWei, Nethereum.Util.UnitConversion.EthUnit.Gwei);

        _logger.LogDebug("Gas price on {Chain}: {GasPrice} Gwei", chain, gasPriceGwei);

        return new GasPrice(
            Chain: chain,
            GasPriceWei: (decimal)gasPriceWei.Value,
            GasPriceGwei: gasPriceGwei,
            FetchedAt: DateTime.UtcNow
        );
    }

    private Web3 GetWeb3Client(string chain)
    {
        var normalizedChain = chain.ToLowerInvariant();

        if (!ChainEndpoints.TryGetValue(normalizedChain, out var baseUrl))
        {
            throw new ArgumentException($"Unsupported chain: {chain}. Supported chains: {string.Join(", ", ChainEndpoints.Keys)}");
        }

        if (!_web3Clients.TryGetValue(normalizedChain, out var client))
        {
            var url = $"{baseUrl}{_apiKey}";
            client = new Web3(url);
            _web3Clients[normalizedChain] = client;
            _logger.LogDebug("Created Web3 client for {Chain}", chain);
        }

        return client;
    }

    private static void ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be null or empty", nameof(address));
        }

        // Basic Ethereum address validation
        if (!address.StartsWith("0x") || address.Length != 42)
        {
            throw new ArgumentException($"Invalid Ethereum address format: {address}", nameof(address));
        }
    }

    private static string ShortenAddress(string address)
    {
        if (address.Length < 12) return address;
        return $"{address[..6]}...{address[^4..]}";
    }
}
