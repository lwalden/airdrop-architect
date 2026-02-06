using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;

namespace AirdropArchitect.Functions.Admin;

/// <summary>
/// Admin function to seed airdrop and points program data into Cosmos DB.
/// Idempotent: uses upsert so can be called multiple times safely.
/// Protected with Function-level auth key.
/// </summary>
public class SeedDataFunction
{
    private readonly IAirdropService _airdropService;
    private readonly IPointsService _pointsService;
    private readonly ILogger<SeedDataFunction> _logger;

    public SeedDataFunction(
        IAirdropService airdropService,
        IPointsService pointsService,
        ILogger<SeedDataFunction> logger)
    {
        _airdropService = airdropService;
        _pointsService = pointsService;
        _logger = logger;
    }

    [Function("SeedData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "admin/seed")]
        HttpRequestData req,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting data seed");

        var airdropCount = 0;
        var programCount = 0;

        try
        {
            // Seed airdrops
            foreach (var airdrop in GetSeedAirdrops())
            {
                await _airdropService.UpsertAirdropAsync(airdrop, ct);
                airdropCount++;
            }

            // Seed points programs
            foreach (var program in GetSeedPointsPrograms())
            {
                await _pointsService.UpsertProgramAsync(program, ct);
                programCount++;
            }

            _logger.LogInformation("Seed complete: {Airdrops} airdrops, {Programs} points programs",
                airdropCount, programCount);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                airdropsSeeded = airdropCount,
                pointsProgramsSeeded = programCount
            }, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { success = false, error = ex.Message }, ct);
            return response;
        }
    }

    private static List<Airdrop> GetSeedAirdrops()
    {
        return new List<Airdrop>
        {
            // === CLAIMABLE ===
            new()
            {
                Id = "starknet-strk",
                Name = "Starknet",
                TokenSymbol = "STRK",
                Chain = "ethereum",
                Status = "claimable",
                ClaimUrl = "https://provisions.starknet.io/",
                ClaimDeadline = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2024, 11, 15, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "Starknet ecosystem user (bridge, dApp interaction)",
                    "Ethereum staker",
                    "Open source contributor"
                },
                TotalEligibleAddresses = 1_300_000,
                AverageAllocationUsd = 450m,
                CheckMethod = "api",
                EligibilityApiUrl = "https://provisions.starknet.io/api/check"
            },
            new()
            {
                Id = "layerzero-zro",
                Name = "LayerZero",
                TokenSymbol = "ZRO",
                Chain = "ethereum",
                Status = "claimable",
                ClaimUrl = "https://layerzero.foundation/eligibility",
                ClaimDeadline = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "Cross-chain message sender via LayerZero",
                    "Stargate Finance user",
                    "Minimum 50 messages across 2+ chains"
                },
                TotalEligibleAddresses = 600_000,
                AverageAllocationUsd = 320m,
                CheckMethod = "merkle",
                EligibilitySource = "https://github.com/LayerZero-Labs/sybil-report"
            },
            new()
            {
                Id = "zksync-zk",
                Name = "ZKSync",
                TokenSymbol = "ZK",
                Chain = "ethereum",
                Status = "claimable",
                ClaimUrl = "https://claim.zknation.io/",
                ClaimDeadline = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2024, 3, 24, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "ZKSync Era transactor",
                    "Bridged assets to ZKSync",
                    "Interacted with ZKSync dApps"
                },
                TotalEligibleAddresses = 695_000,
                AverageAllocationUsd = 280m,
                CheckMethod = "merkle"
            },
            new()
            {
                Id = "wormhole-w",
                Name = "Wormhole",
                TokenSymbol = "W",
                Chain = "ethereum",
                Status = "claimable",
                ClaimUrl = "https://wormhole.com/airdrop",
                ClaimDeadline = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2024, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "Cross-chain transfers via Wormhole",
                    "Portal Bridge user",
                    "Minimum 3 bridge transactions"
                },
                TotalEligibleAddresses = 400_000,
                AverageAllocationUsd = 200m,
                CheckMethod = "api"
            },

            // === UPCOMING ===
            new()
            {
                Id = "scroll-scr",
                Name = "Scroll",
                TokenSymbol = "SCR",
                Chain = "ethereum",
                Status = "upcoming",
                Criteria = new List<string>
                {
                    "Scroll Marks holder (Session 1-3)",
                    "Deployed contracts on Scroll",
                    "Active DeFi participation on Scroll"
                },
                CheckMethod = "manual",
                EligibilitySource = "Scroll Marks dashboard"
            },
            new()
            {
                Id = "linea-lxp",
                Name = "Linea",
                TokenSymbol = "LXP",
                Chain = "ethereum",
                Status = "upcoming",
                Criteria = new List<string>
                {
                    "Linea Voyage participant",
                    "LXP token holder",
                    "Bridged to Linea mainnet"
                },
                CheckMethod = "manual",
                EligibilitySource = "Linea Voyage dashboard"
            },
            new()
            {
                Id = "debridge-dbr",
                Name = "deBridge",
                TokenSymbol = "DBR",
                Chain = "ethereum",
                Status = "upcoming",
                Criteria = new List<string>
                {
                    "deBridge cross-chain transfer user",
                    "deSwap user",
                    "Points accumulated via deBridge Points"
                },
                CheckMethod = "api"
            },

            // === EXPIRED (for testing historical data) ===
            new()
            {
                Id = "arbitrum-arb",
                Name = "Arbitrum",
                TokenSymbol = "ARB",
                Chain = "arbitrum",
                Status = "expired",
                ClaimUrl = "https://arbitrum.foundation/airdrop",
                ClaimDeadline = new DateTime(2024, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2023, 2, 6, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "Arbitrum One bridge user",
                    "Conducted transactions on Arbitrum One",
                    "Bridged before cutoff date"
                },
                TotalEligibleAddresses = 625_000,
                AverageAllocationUsd = 2100m,
                CheckMethod = "merkle"
            },
            new()
            {
                Id = "optimism-op",
                Name = "Optimism",
                TokenSymbol = "OP",
                Chain = "optimism",
                Status = "expired",
                ClaimUrl = "https://app.optimism.io/airdrop/check",
                ClaimDeadline = new DateTime(2023, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2022, 3, 25, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "Optimism early user (bridge + transactions)",
                    "Repeat Optimism user",
                    "DAO voter",
                    "Multi-sig signer"
                },
                TotalEligibleAddresses = 248_000,
                AverageAllocationUsd = 1200m,
                CheckMethod = "merkle"
            },
            new()
            {
                Id = "blur-blur",
                Name = "Blur",
                TokenSymbol = "BLUR",
                Chain = "ethereum",
                Status = "expired",
                ClaimUrl = "https://blur.io/airdrop",
                ClaimDeadline = new DateTime(2024, 6, 14, 0, 0, 0, DateTimeKind.Utc),
                SnapshotDate = new DateTime(2023, 2, 14, 0, 0, 0, DateTimeKind.Utc),
                Criteria = new List<string>
                {
                    "NFT trader on Blur marketplace",
                    "Listed NFTs on Blur",
                    "Placed bids on Blur"
                },
                TotalEligibleAddresses = 124_000,
                AverageAllocationUsd = 1850m,
                CheckMethod = "merkle"
            }
        };
    }

    private static List<PointsProgram> GetSeedPointsPrograms()
    {
        return new List<PointsProgram>
        {
            new()
            {
                Id = "hyperliquid-points",
                ProtocolName = "Hyperliquid",
                PointsName = "Points",
                Chain = "arbitrum",
                Status = "active",
                DashboardUrl = "https://app.hyperliquid.xyz/points",
                ApiEndpoint = "https://api.hyperliquid.xyz/info",
                TrackingMethod = "api",
                EstimatedTgeDate = "TBD",
                TokenSymbol = "HYPE"
            },
            new()
            {
                Id = "eigenlayer-points",
                ProtocolName = "EigenLayer",
                PointsName = "Restaked Points",
                Chain = "ethereum",
                Status = "active",
                DashboardUrl = "https://app.eigenlayer.xyz/",
                ApiEndpoint = "https://claims.eigenfoundation.org/clique-eigenlayer-api",
                TrackingMethod = "api",
                EstimatedTgeDate = "Season 3 ongoing",
                TokenSymbol = "EIGEN"
            },
            new()
            {
                Id = "ethena-sats",
                ProtocolName = "Ethena",
                PointsName = "Sats",
                Chain = "ethereum",
                Status = "active",
                DashboardUrl = "https://app.ethena.fi/earn",
                ApiEndpoint = "https://app.ethena.fi/api/users/points",
                TrackingMethod = "api",
                EstimatedTgeDate = "Season 3 ongoing",
                TokenSymbol = "ENA"
            },
            new()
            {
                Id = "scroll-marks",
                ProtocolName = "Scroll",
                PointsName = "Marks",
                Chain = "ethereum",
                Status = "active",
                DashboardUrl = "https://scroll.io/sessions",
                TrackingMethod = "manual",
                EstimatedTgeDate = "2026 H1"
            }
        };
    }
}
