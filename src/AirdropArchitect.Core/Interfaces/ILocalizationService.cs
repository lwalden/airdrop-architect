namespace AirdropArchitect.Core.Interfaces;

/// <summary>
/// Service for retrieving localized strings from locale files.
/// See ADR-011 for i18n strategy and locale file structure.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    /// <param name="key">The string key (e.g., "Welcome", "InvalidAddress")</param>
    /// <param name="languageCode">ISO 639-1 language code (e.g., "en", "es"). Defaults to "en".</param>
    /// <returns>The localized string, or the key itself if not found</returns>
    string Get(string key, string? languageCode = null);

    /// <summary>
    /// Gets a localized string with format parameters.
    /// </summary>
    /// <param name="key">The string key</param>
    /// <param name="languageCode">ISO 639-1 language code</param>
    /// <param name="args">Format arguments for string.Format</param>
    /// <returns>The formatted localized string</returns>
    string GetFormatted(string key, string? languageCode, params object[] args);

    /// <summary>
    /// Gets a localized string from a specific category.
    /// </summary>
    /// <param name="category">The category (e.g., "telegram", "errors", "notifications")</param>
    /// <param name="key">The string key</param>
    /// <param name="languageCode">ISO 639-1 language code</param>
    /// <returns>The localized string</returns>
    string Get(string category, string key, string? languageCode = null);

    /// <summary>
    /// Checks if a language is supported.
    /// </summary>
    /// <param name="languageCode">ISO 639-1 language code</param>
    /// <returns>True if the language has locale files</returns>
    bool IsLanguageSupported(string languageCode);

    /// <summary>
    /// Gets the list of supported language codes.
    /// </summary>
    IReadOnlyList<string> GetSupportedLanguages();

    /// <summary>
    /// Gets the default language code.
    /// </summary>
    string DefaultLanguage { get; }
}
