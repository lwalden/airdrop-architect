namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for checking geographic restrictions based on OFAC sanctions and other regulations.
/// See ADR-011 for blocked countries list and rationale.
/// </summary>
public interface IGeoRestrictionService
{
    /// <summary>
    /// Checks if a country is allowed to use the service.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code (e.g., "US", "IR")</param>
    /// <returns>True if the country is allowed, false if blocked</returns>
    bool IsCountryAllowed(string countryCode);

    /// <summary>
    /// Gets the list of blocked country codes.
    /// </summary>
    IReadOnlyList<string> GetBlockedCountries();

    /// <summary>
    /// Gets the reason a country is blocked (for logging/audit purposes).
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code</param>
    /// <returns>Reason string or null if not blocked</returns>
    string? GetBlockedReason(string countryCode);
}
