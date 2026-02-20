# Internationalization (i18n) & Geographic Restrictions

> Extracted from CLAUDE.md during AIAgentMinder v0.5.3 update.
> Reference on-demand. See ADR-011 in DECISIONS.md for geo-restriction rationale.

---

## Core Principles

**IMPORTANT:** All code must be written with future localization in mind. MVP is English-only, but the architecture must support adding languages without major refactoring.

1. **Never hardcode user-facing strings in code**
   - Bad: `await SendMessageAsync("Welcome to Airdrop Architect!")`
   - Good: `await SendMessageAsync(_localizer["Welcome"])`

2. **Use locale files for all text**
   - Store strings in `/src/AirdropArchitect.Core/Locales/` directory
   - JSON format: `en.json`, `es.json`, etc.
   - Organize by feature: `telegram.json`, `errors.json`, `notifications.json`

3. **Telegram bot: Read and store user language**
   - Telegram provides `language_code` in user messages
   - Store in User model: `PreferredLanguage` field
   - Fall back to English if unavailable

---

## Locale File Structure

```
/src/AirdropArchitect.Core/Locales/
├── en/
│   ├── telegram.json       # Bot command responses
│   ├── errors.json         # Error messages
│   └── notifications.json  # Alert templates
└── (future: es/, pt/, zh/)
```

### Example Locale File (`en/telegram.json`)

```json
{
  "Welcome": "Welcome to Airdrop Architect!\n\nI help you find unclaimed airdrops and track points across your wallets.",
  "CheckingWallet": "Scanning wallet for airdrops...",
  "NoEligibleAirdrops": "No eligible airdrops found for this wallet.",
  "PointsHeader": "Points for {address}",
  "UpgradePrompt": "Use /upgrade to unlock more features!",
  "InvalidAddress": "Invalid wallet address. Please check and try again.",
  "ServiceUnavailable": "Service temporarily unavailable. Please try again later."
}
```

---

## Code Pattern for Localized Messages

```csharp
// ILocalizationService interface
public interface ILocalizationService
{
    string Get(string key, string? languageCode = null);
    string Get(string key, string languageCode, params object[] args);
}

// Usage in TelegramBotService
await _bot.SendTextMessageAsync(
    chatId,
    _localizer.Get("Welcome", user.PreferredLanguage),
    cancellationToken: ct);

// With string interpolation
await _bot.SendTextMessageAsync(
    chatId,
    _localizer.Get("PointsHeader", user.PreferredLanguage, ShortenAddress(address)),
    cancellationToken: ct);
```

---

## Geographic Restrictions

See ADR-011 in DECISIONS.md for full rationale.

**Blocked Countries (OFAC + Algeria):**
- Iran (IR), North Korea (KP), Syria (SY), Cuba (CU)
- Russia (RU), Belarus (BY), Venezuela (VE), Afghanistan (AF)
- Algeria (DZ) — crypto banned July 2025

**Implementation:**
- `IGeoRestrictionService.IsAllowedAsync(string countryCode)`
- Configurable blocklist in app settings (not hardcoded)
- Check at Telegram webhook entry point
- Store user country in User model for analytics
