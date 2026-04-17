using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace WC.Helpers
{
    public enum WarningSeverity
    {
        Critical,
        High,
        Medium,
        Low,
        Unknown
    }

    public readonly struct WarningClassification
    {
        public WarningSeverity Severity { get; }
        public string CanonicalText { get; }

        public WarningClassification(WarningSeverity severity, string canonicalText)
        {
            Severity = severity;
            CanonicalText = canonicalText;
        }
    }

    public static class WarningSeverityClassifier
    {
        private const string ResourceName = "WC.Resources.RevitWarningsClassified.json";

        // Revit substitutes element identifiers into warning text in three observed shapes:
        //   1. Quoted placeholders   ("Type Name", "View Name")
        //   2. Leading bare nouns     ("Room is not...", "Space Type is not...")
        //   3. Trailing bare nouns    ("...for Room is set correctly")
        // We can't anchor on prefix. For each canonical, we generate several candidate needles
        // (sentence splits, quoted-segment splits, verb-anchored variants) and match by
        // longest-needle-contained-in-warning.
        private const int MinNeedleLength = 25;

        // Words that signal "the variable element name is over, the stable wording starts here".
        private static readonly string[] s_verbAnchors =
        {
            " is ", " are ", " has ", " have ", " was ", " were ",
            " cannot ", " can't ", " could ", " may ", " might ", " must ",
            " will ", " won't ", " should ", " shouldn't ",
            " do ", " does ", " don't ", " doesn't ",
            " contains ", " exceeds ", " requires ", " needs "
        };

        private sealed class Entry
        {
            public string Canonical;   // human-readable label used in tooltip breakdown
            public string Needle;      // substring to look for in the live warning
            public WarningSeverity Severity;
        }

        private static List<Entry> _entries; // sorted by Needle.Length descending
        private static readonly object _lock = new object();

        public static WarningSeverity GetSeverity(string description) => Classify(description).Severity;

        public static WarningClassification Classify(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return new WarningClassification(WarningSeverity.Unknown, description ?? string.Empty);

            EnsureLoaded();
            var normalized = Normalize(description);

            foreach (var e in _entries)
            {
                if (normalized.Length < e.Needle.Length) continue;
                if (normalized.IndexOf(e.Needle, StringComparison.Ordinal) >= 0)
                    return new WarningClassification(e.Severity, e.Canonical);
            }

            return new WarningClassification(WarningSeverity.Unknown, description.Trim());
        }

        private static void EnsureLoaded()
        {
            if (_entries != null) return;
            lock (_lock)
            {
                if (_entries != null) return;

                var list = new List<Entry>();
                try
                {
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
                    if (stream == null) { _entries = list; return; }

                    using var doc = JsonDocument.Parse(stream);
                    foreach (var bucket in doc.RootElement.EnumerateObject())
                    {
                        if (!Enum.TryParse<WarningSeverity>(bucket.Name, ignoreCase: true, out var sev))
                            continue;
                        foreach (var item in bucket.Value.EnumerateArray())
                        {
                            var raw = item.GetString();
                            if (string.IsNullOrWhiteSpace(raw)) continue;

                            var canonical = StripTrailingEllipsis(raw.Trim());
                            foreach (var needle in ExtractNeedles(canonical))
                            {
                                list.Add(new Entry { Canonical = canonical, Needle = needle, Severity = sev });
                            }
                        }
                    }
                }
                catch
                {
                    // Best-effort: if the resource is missing or malformed, every warning falls back to Unknown.
                }

                list.Sort((a, b) => b.Needle.Length.CompareTo(a.Needle.Length));
                _entries = list;
            }
        }

        private static IEnumerable<string> ExtractNeedles(string canonical)
        {
            var sentences = canonical.Split(
                new[] { ". ", ".\r\n", ".\n", "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            if (sentences.Length == 0) sentences = new[] { canonical };

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var sentence in sentences)
            {
                foreach (var part in SplitOnQuoted(sentence))
                {
                    var trimmed = Normalize(part).TrimEnd('.', ',', ';', ':').Trim();
                    if (trimmed.Length >= MinNeedleLength && seen.Add(trimmed))
                        yield return trimmed;

                    var anchored = AnchorAtFirstVerb(trimmed);
                    if (anchored != null && anchored.Length >= MinNeedleLength && seen.Add(anchored))
                        yield return anchored;
                }
            }
        }

        private static string AnchorAtFirstVerb(string s)
        {
            int bestIdx = -1;
            foreach (var a in s_verbAnchors)
            {
                var idx = s.IndexOf(a, StringComparison.OrdinalIgnoreCase);
                if (idx > 0 && (bestIdx == -1 || idx < bestIdx))
                    bestIdx = idx;
            }
            if (bestIdx < 1) return null;
            return s.Substring(bestIdx + 1); // skip leading space, keep the verb
        }

        private static IEnumerable<string> SplitOnQuoted(string s)
        {
            var current = new StringBuilder();
            bool inQuote = false;
            foreach (var ch in s)
            {
                if (ch == '"')
                {
                    if (current.Length > 0) { yield return current.ToString(); current.Clear(); }
                    inQuote = !inQuote;
                }
                else if (!inQuote)
                {
                    current.Append(ch);
                }
            }
            if (current.Length > 0) yield return current.ToString();
        }

        private static string Normalize(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool prevWs = false;
            foreach (var ch in s)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!prevWs && sb.Length > 0) sb.Append(' ');
                    prevWs = true;
                }
                else
                {
                    sb.Append(ch);
                    prevWs = false;
                }
            }
            while (sb.Length > 0 && sb[sb.Length - 1] == ' ') sb.Length--;
            return sb.ToString();
        }

        private static string StripTrailingEllipsis(string s)
        {
            var trimmed = s.TrimEnd();
            if (trimmed.EndsWith("...", StringComparison.Ordinal))
                return trimmed.Substring(0, trimmed.Length - 3).TrimEnd();
            if (trimmed.EndsWith("\u2026", StringComparison.Ordinal))
                return trimmed.Substring(0, trimmed.Length - 1).TrimEnd();
            return trimmed;
        }
    }
}
