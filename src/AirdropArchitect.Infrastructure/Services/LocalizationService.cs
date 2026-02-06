using System.Collections.Concurrent;
using System.Text.Json;
using AirdropArchitect.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirdropArchitect.Infrastructure.Services;

/// <summary>
/// Implementation of localization service using JSON locale files.
/// See ADR-011 for i18n strategy.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly string _localesPath;
    private readonly ConcurrentDictionary<string, Dictionary<string, Dictionary<string, string>>> _localeCache;
    private readonly HashSet<string> _supportedLanguages;

    private const string DefaultLanguageCode = "en";

    public string DefaultLanguage => DefaultLanguageCode;

    public LocalizationService(ILogger<LocalizationService> logger, string? localesPath = null)
    {
        _logger = logger;
        _localesPath = localesPath ?? GetDefaultLocalesPath();
        _localeCache = new ConcurrentDictionary<string, Dictionary<string, Dictionary<string, string>>>();
        _supportedLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        LoadAvailableLanguages();
    }

    /// <inheritdoc />
    public string Get(string key, string? languageCode = null)
    {
        return Get("telegram", key, languageCode);
    }

    /// <inheritdoc />
    public string GetFormatted(string key, string? languageCode, params object[] args)
    {
        var template = Get(key, languageCode);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "Failed to format localized string '{Key}' with {ArgCount} args", key, args.Length);
            return template;
        }
    }

    /// <inheritdoc />
    public string Get(string category, string key, string? languageCode = null)
    {
        var lang = NormalizeLanguageCode(languageCode);

        // Try requested language first
        var value = GetFromCache(lang, category, key);
        if (value != null)
        {
            return value;
        }

        // Fall back to default language if different
        if (!string.Equals(lang, DefaultLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            value = GetFromCache(DefaultLanguageCode, category, key);
            if (value != null)
            {
                return value;
            }
        }

        // Return the key itself as last resort (helps identify missing translations)
        _logger.LogDebug("Missing localization: [{Language}] {Category}.{Key}", lang, category, key);
        return key;
    }

    /// <inheritdoc />
    public bool IsLanguageSupported(string languageCode)
    {
        var normalized = NormalizeLanguageCode(languageCode);
        return _supportedLanguages.Contains(normalized);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetSupportedLanguages()
    {
        return _supportedLanguages.ToList().AsReadOnly();
    }

    private string? GetFromCache(string language, string category, string key)
    {
        var langData = GetOrLoadLanguage(language);
        if (langData == null)
        {
            return null;
        }

        if (langData.TryGetValue(category, out var categoryData) &&
            categoryData.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    private Dictionary<string, Dictionary<string, string>>? GetOrLoadLanguage(string language)
    {
        return _localeCache.GetOrAdd(language, lang =>
        {
            var langData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var langPath = Path.Combine(_localesPath, lang);

            if (!Directory.Exists(langPath))
            {
                _logger.LogDebug("Locale directory not found: {Path}", langPath);
                return langData;
            }

            foreach (var file in Directory.GetFiles(langPath, "*.json"))
            {
                try
                {
                    var category = Path.GetFileNameWithoutExtension(file);
                    var json = File.ReadAllText(file);
                    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (data != null)
                    {
                        langData[category] = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
                        _logger.LogDebug("Loaded locale file: {File} with {Count} keys", file, data.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load locale file: {File}", file);
                }
            }

            return langData;
        });
    }

    private void LoadAvailableLanguages()
    {
        if (!Directory.Exists(_localesPath))
        {
            _logger.LogWarning("Locales directory not found: {Path}. Creating with default English.", _localesPath);
            Directory.CreateDirectory(_localesPath);
            _supportedLanguages.Add(DefaultLanguageCode);
            return;
        }

        foreach (var dir in Directory.GetDirectories(_localesPath))
        {
            var langCode = Path.GetFileName(dir);
            _supportedLanguages.Add(langCode);
        }

        // Always ensure English is supported
        if (!_supportedLanguages.Contains(DefaultLanguageCode))
        {
            _supportedLanguages.Add(DefaultLanguageCode);
        }

        _logger.LogInformation(
            "LocalizationService initialized with {Count} languages: {Languages}",
            _supportedLanguages.Count,
            string.Join(", ", _supportedLanguages));
    }

    private static string NormalizeLanguageCode(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguageCode;
        }

        // Handle codes like "en-US" -> "en"
        var normalized = languageCode.Split('-')[0].ToLowerInvariant();

        return normalized;
    }

    private static string GetDefaultLocalesPath()
    {
        // Try to find the Locales folder relative to the executing assembly
        var assemblyLocation = AppContext.BaseDirectory;
        var localesPath = Path.Combine(assemblyLocation, "Locales");

        if (Directory.Exists(localesPath))
        {
            return localesPath;
        }

        // Fallback to current directory
        return Path.Combine(Directory.GetCurrentDirectory(), "Locales");
    }
}
