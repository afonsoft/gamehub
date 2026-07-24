using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameHub.Moderation
{
    /// <summary>
    /// Simple profanity filter supporting Portuguese and English blocklists with basic leet detection.
    /// </summary>
    public class ProfanityFilter
    {
        private static readonly string[] Blocklist = new[]
        {
            // Portuguese
            "caralho", "porra", "merda", "puta", "viado", "bosta", "cu", "cuzão",
            "filho da puta", "desgraçado", "babaca", "idiota", "imbecil", "retardado",
            // English
            "fuck", "shit", "bitch", "asshole", "damn", "cunt", "dick", "pussy",
            "motherfucker", "bastard", "retard"
        };

        private static readonly Dictionary<char, char> LeetMap = new Dictionary<char, char>
        {
            ['@'] = 'a',
            ['4'] = 'a',
            ['3'] = 'e',
            ['1'] = 'i',
            ['!'] = 'i',
            ['0'] = 'o',
            ['5'] = 's',
            ['$'] = 's',
            ['7'] = 't',
            ['9'] = 'g'
        };

        private static readonly Regex WordSeparatorRegex = new(@"[^a-zA-Z0-9áéíóúãõâêôç]", RegexOptions.Compiled);

        public bool ContainsProfanity(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var normalized = Normalize(input);
            var words = WordSeparatorRegex.Split(normalized).Where(w => !string.IsNullOrWhiteSpace(w));

            foreach (var word in words)
            {
                foreach (var term in Blocklist)
                {
                    if (term.Split(' ').Length > 1)
                    {
                        if (normalized.Contains(term, StringComparison.OrdinalIgnoreCase))
                            return true;
                        continue;
                    }

                    if (word.Contains(term, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public string Censor(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var result = input;
            var normalizedInput = Normalize(input);

            foreach (var term in Blocklist)
            {
                var pattern = BuildRegexForTerm(term);
                result = Regex.Replace(result, pattern, m => new string('*', m.Length), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            return result;
        }

        private static string BuildRegexForTerm(string term)
        {
            if (term.Contains(' '))
            {
                return @"\b" + Regex.Escape(term).Replace(" ", @"[^a-zA-Z0-9]*") + @"\b";
            }

            var chars = term.ToCharArray();
            var patternChars = new List<string>();
            foreach (var c in chars)
            {
                var variants = new List<char> { c };
                variants.AddRange(LeetMap.Where(kvp => kvp.Value == c).Select(kvp => kvp.Key));
                patternChars.Add($"[{string.Join(string.Empty, variants.Distinct())}]");
            }

            return @"\b" + string.Join(@"[^a-zA-Z0-9]*", patternChars) + @"\b";
        }

        private static string Normalize(string input)
        {
            var chars = input.ToLowerInvariant().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (LeetMap.TryGetValue(chars[i], out var mapped))
                {
                    chars[i] = mapped;
                }
            }
            return new string(chars);
        }
    }
}
