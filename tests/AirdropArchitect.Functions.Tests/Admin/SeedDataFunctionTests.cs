using System.Net;
using System.Text.Json;
using AirdropArchitect.Core.Interfaces;
using AirdropArchitect.Core.Models;
using AirdropArchitect.Functions.Admin;
using AirdropArchitect.Functions.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace AirdropArchitect.Functions.Tests.Admin;

public class SeedDataFunctionTests
{
    [Fact]
    public async Task Run_ReturnsOk_AndSeedsExpectedRecordCounts()
    {
        var airdropService = new FakeAirdropService();
        var pointsService = new FakePointsService();
        var function = new SeedDataFunction(
            airdropService,
            pointsService,
            NullLogger<SeedDataFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(10, airdropService.UpsertCallCount);
        Assert.Equal(4, pointsService.UpsertCallCount);

        var payload = FunctionHttpTestFactory.ReadBodyAsString(response);
        using var json = JsonDocument.Parse(payload);

        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(10, json.RootElement.GetProperty("airdropsSeeded").GetInt32());
        Assert.Equal(4, json.RootElement.GetProperty("pointsProgramsSeeded").GetInt32());
    }

    [Fact]
    public async Task Run_ReturnsInternalServerError_WhenAirdropSeedingFails()
    {
        var airdropService = new FakeAirdropService
        {
            ExceptionToThrow = new InvalidOperationException("seed failed")
        };
        var pointsService = new FakePointsService();
        var function = new SeedDataFunction(
            airdropService,
            pointsService,
            NullLogger<SeedDataFunction>.Instance);
        var request = FunctionHttpTestFactory.CreateHttpRequest(body: "{}");

        var response = await function.Run(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, airdropService.UpsertCallCount);
        Assert.Equal(0, pointsService.UpsertCallCount);

        var payload = FunctionHttpTestFactory.ReadBodyAsString(response);
        using var json = JsonDocument.Parse(payload);

        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("seed failed", json.RootElement.GetProperty("error").GetString());
    }

    private sealed class FakeAirdropService : IAirdropService
    {
        public int UpsertCallCount { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<Airdrop> UpsertAirdropAsync(Airdrop airdrop, CancellationToken ct = default)
        {
            UpsertCallCount++;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(airdrop);
        }

        public Task<List<Airdrop>> GetActiveAirdropsAsync(CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<Airdrop?> GetAirdropAsync(string airdropId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<List<EligibilityCheck>> CheckEligibilityAsync(string walletAddress, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<EligibilityCheck> CheckEligibilityForAirdropAsync(string walletAddress, string airdropId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<List<EligibilityResult>> GetCachedEligibilityAsync(string walletAddress, CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    private sealed class FakePointsService : IPointsService
    {
        public int UpsertCallCount { get; private set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<PointsProgram> UpsertProgramAsync(PointsProgram program, CancellationToken ct = default)
        {
            UpsertCallCount++;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(program);
        }

        public Task<List<PointsProgram>> GetActiveProgramsAsync(CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<PointsProgram?> GetProgramAsync(string programId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<List<PointsBalance>> GetPointsForWalletAsync(string walletAddress, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<PointsBalance?> GetPointsForProgramAsync(string walletAddress, string programId, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<List<PointsBalance>> RefreshPointsAsync(string walletAddress, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<List<PointsSnapshot>> GetSnapshotHistoryAsync(string walletAddress, string? programId = null, int limit = 30, CancellationToken ct = default)
            => throw new NotSupportedException();
    }
}
