using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Services;

/// <summary>
/// Implementation of geographic restriction service for OFAC compliance.
/// Blocked countries list based on ADR-011.
/// </summary>
public class GeoRestrictionService : IGeoRestrictionService
{
    private readonly ILogger<GeoRestrictionService> _logger;
    private readonly Dictionary<string, string> _blockedCountries;

    /// <summary>
    /// Default blocked countries based on OFAC sanctions and crypto regulations.
    /// See ADR-011 for rationale.
    /// </summary>
    private static readonly Dictionary<string, string> DefaultBlockedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        // OFAC Comprehensive Sanctions
        { "IR", "OFAC Comprehensive Sanctions - Iran" },
        { "KP", "OFAC Comprehensive Sanctions - North Korea" },
        { "SY", "OFAC Comprehensive Sanctions - Syria" },
        { "CU", "OFAC Comprehensive Sanctions - Cuba" },

        // OFAC Significant Restrictions
        { "RU", "OFAC Significant Restrictions - Russia" },
        { "BY", "OFAC Significant Restrictions - Belarus" },
        { "VE", "OFAC Significant Restrictions - Venezuela" },

        // OFAC Partial Sanctions
        { "AF", "OFAC Partial Sanctions - Afghanistan" },

        // Crypto-specific bans
        { "DZ", "Crypto activities banned - Algeria (July 2025)" },

        // OFAC - Crimea Region (using unofficial code)
        { "UA-43", "OFAC Sanctions - Crimea Region" }
    };

    public GeoRestrictionService(ILogger<GeoRestrictionService> logger)
        : this(logger, null)
    {
    }

    public GeoRestrictionService(
        ILogger<GeoRestrictionService> logger,
        IEnumerable<KeyValuePair<string, string>>? additionalBlockedCountries)
    {
        _logger = logger;
        _blockedCountries = new Dictionary<string, string>(DefaultBlockedCountries, StringComparer.OrdinalIgnoreCase);

        // Allow configuration to add more blocked countries
        if (additionalBlockedCountries != null)
        {
            foreach (var country in additionalBlockedCountries)
            {
                _blockedCountries[country.Key] = country.Value;
            }
        }

        _logger.LogInformation(
            "GeoRestrictionService initialized with {Count} blocked countries",
            _blockedCountries.Count);
    }

    /// <inheritdoc />
    public bool IsCountryAllowed(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            _logger.LogWarning("Empty country code provided to IsCountryAllowed");
            return true; // Allow if no country code (user hasn't been geo-located yet)
        }

        var isBlocked = _blockedCountries.ContainsKey(countryCode);

        if (isBlocked)
        {
            _logger.LogInformation(
                "Access blocked for country code {CountryCode}: {Reason}",
                countryCode,
                _blockedCountries[countryCode]);
        }

        return !isBlocked;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetBlockedCountries()
    {
        return _blockedCountries.Keys.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public string? GetBlockedReason(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        return _blockedCountries.TryGetValue(countryCode, out var reason) ? reason : null;
    }
}
