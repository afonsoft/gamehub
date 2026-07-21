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
    public class AgeRating : ValueObject
    {
        public string Value { get; }

        private static readonly string[] ValidRatings = { "Everyone", "Teen", "Mature" };

        private AgeRating(string value) => Value = value;

        public static AgeRating Create(string rating)
        {
            if (!ValidRatings.Contains(rating, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid age rating: {rating}. Valid: {string.Join(", ", ValidRatings)}");

            return new AgeRating(ValidRatings.First(r =>
                r.Equals(rating, StringComparison.OrdinalIgnoreCase)));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
