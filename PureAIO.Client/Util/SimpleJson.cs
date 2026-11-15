// Minimal zero-dependency JSON field extractor for .NET 4.7.2.
// Not a full parser — extracts named scalar fields and one named block.
// Sufficient for the flat API responses we consume (Roblox / Discord / Exodus).

using System;
using System.Text.RegularExpressions;

namespace PureAIO.Client.Util
{
    public static class SimpleJson
    {
        // ── Scalar getters ─────────────────────────────────────────────────────

        /// <summary>Returns the string value of a JSON field, or null.</summary>
        public static string GetString(string json, string field)
        {
            if (json == null) return null;
            // "field":"value" or "field": "value" — handles null too
            var m = Regex.Match(json,
                @"""" + Regex.Escape(field) + @"""\s*:\s*(?:""((?:[^""\\]|\\.)*)""|(null))",
                RegexOptions.Singleline);
            if (!m.Success) return null;
            if (m.Groups[2].Value == "null") return null;
            return Unescape(m.Groups[1].Value);
        }

        /// <summary>Returns a long integer field value, or 0.</summary>
        public static long GetLong(string json, string field)
        {
            if (json == null) return 0;
            var m = Regex.Match(json,
                @"""" + Regex.Escape(field) + @"""\s*:\s*(-?\d+)");
            if (!m.Success) return 0;
            return long.TryParse(m.Groups[1].Value, out var v) ? v : 0;
        }

        /// <summary>Returns a boolean field value.</summary>
        public static bool GetBool(string json, string field)
        {
            if (json == null) return false;
            var m = Regex.Match(json,
                @"""" + Regex.Escape(field) + @"""\s*:\s*(true|false)");
            if (!m.Success) return false;
            return m.Groups[1].Value == "true";
        }

        // ── Block extractor ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the raw JSON content of a named object block { ... }.
        /// E.g. ExtractBlock(json, "assets") returns the value of the "assets" key.
        /// Handles one level of nesting only — sufficient for Exodus info.json.
        /// </summary>
        public static string ExtractBlock(string json, string field)
        {
            if (json == null) return null;
            var key = "\"" + field + "\"";
            int idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return null;

            // Skip : and whitespace
            int pos = idx + key.Length;
            while (pos < json.Length && (json[pos] == ' ' || json[pos] == '\t'
                                      || json[pos] == '\r' || json[pos] == '\n'
                                      || json[pos] == ':'))
                pos++;

            if (pos >= json.Length || json[pos] != '{') return null;

            // Find matching closing brace
            int depth = 0;
            int start = pos;
            for (int i = pos; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}') { depth--; if (depth == 0) return json.Substring(start, i - start + 1); }
            }
            return null;
        }

        // ── Helper ─────────────────────────────────────────────────────────────
        private static string Unescape(string s)
        {
            if (!s.Contains("\\")) return s;
            return s
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\/",  "/")
                .Replace("\\n",  "\n")
                .Replace("\\r",  "\r")
                .Replace("\\t",  "\t");
        }
    }
}
