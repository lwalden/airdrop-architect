using AirdropArchitect.Core.Models;

namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for tracking points programs and wallet balances
/// </summary>
public interface IPointsService
{
    /// <summary>
    /// Get all active points programs
    /// </summary>
    Task<List<PointsProgram>> GetActiveProgramsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific points program by ID
    /// </summary>
    Task<PointsProgram?> GetProgramAsync(string programId, CancellationToken ct = default);

    /// <summary>
    /// Get points balances for a wallet across all active programs
    /// </summary>
    Task<List<PointsBalance>> GetPointsForWalletAsync(
        string walletAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Get points balance for a wallet in a specific program
    /// </summary>
    Task<PointsBalance?> GetPointsForProgramAsync(
        string walletAddress,
        string programId,
        CancellationToken ct = default);

    /// <summary>
    /// Refresh points for a wallet (fetch latest from APIs)
    /// </summary>
    Task<List<PointsBalance>> RefreshPointsAsync(
        string walletAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Get historical snapshots for a wallet
    /// </summary>
    Task<List<PointsSnapshot>> GetSnapshotHistoryAsync(
        string walletAddress,
        string? programId = null,
        int limit = 30,
        CancellationToken ct = default);

    /// <summary>
    /// Add or update a points program (admin function)
    /// </summary>
    Task<PointsProgram> UpsertProgramAsync(PointsProgram program, CancellationToken ct = default);
}

/// <summary>
/// Current points balance for a wallet in a program
/// </summary>
public record PointsBalance(
    string ProgramId,
    string ProtocolName,
    string PointsName,
    decimal Points,
    int? Rank,
    decimal? Percentile,
    decimal? EstimatedValueUsd,
    decimal? PointsChange24h,
    string? DashboardUrl,
    DateTime LastUpdated
);
