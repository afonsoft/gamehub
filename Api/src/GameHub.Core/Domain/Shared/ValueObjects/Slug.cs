using GameHub.Moderation;
using GameHub.Gameplay;
using GameHub.Developers;
using GameHub.Catalog;
using GameHub.Builds;
using GameHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Abp.Domain.Values;

namespace GameHub
{
    public class Slug : ValueObject
    {
        public string Value { get; }

        private Slug(string value)
        {
            Value = value;
        }

        public static Slug Create(string input)
        {
            var normalized = input
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "-")
                .Replace("_", "-");

            // Remove caracteres especiais, mantém apenas [a-z0-9-]
            normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "");

            // Remove múltiplos hífens
            normalized = Regex.Replace(normalized, @"-{2,}", "-");

            // Remove hífens no início e fim
            normalized = normalized.Trim('-');

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException("Slug cannot be empty.");

            if (normalized.Length > 256)
                throw new ArgumentException("Slug cannot exceed 256 characters.");

            return new Slug(normalized);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
