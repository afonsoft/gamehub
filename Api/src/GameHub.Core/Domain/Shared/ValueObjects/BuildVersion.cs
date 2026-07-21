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
    public class BuildVersion : ValueObject
    {
        public string Value { get; }

        private BuildVersion(string value) => Value = value;

        public static BuildVersion Create(string version)
        {
            if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
                throw new ArgumentException($"Invalid semver: {version}. Expected format: MAJOR.MINOR.PATCH");

            return new BuildVersion(version);
        }

        public BuildVersion IncrementMajor() =>
            ParseParts((major, minor, patch) => new BuildVersion($"{major + 1}.0.0"));

        public BuildVersion IncrementMinor() =>
            ParseParts((major, minor, patch) => new BuildVersion($"{major}.{minor + 1}.0"));

        public BuildVersion IncrementPatch() =>
            ParseParts((major, minor, patch) => new BuildVersion($"{major}.{minor}.{patch + 1}"));

        private T ParseParts<T>(Func<int, int, int, T> func)
        {
            var parts = Value.Split('.').Select(int.Parse).ToArray();
            return func(parts[0], parts[1], parts[2]);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
