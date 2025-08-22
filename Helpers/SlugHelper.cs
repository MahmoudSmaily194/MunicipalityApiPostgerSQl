﻿using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SawirahMunicipalityWeb.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // Normalize string (decompose)
            string normalized = phrase.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    if (IsEnglishLetterOrDigit(c) || IsArabicLetter(c) || c == ' ')
                        stringBuilder.Append(c);
                }
            }

            string slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
            // Remove any character that is not English letter, Arabic letter, number, or hyphen
            slug = Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF-]", "");
            // Replace multiple hyphens with single hyphen
            slug = Regex.Replace(slug, @"-+", "-");

            // ✅ Limit slug length (safe for PostgreSQL identifiers)
            const int maxLength = 60;
            if (slug.Length > maxLength)
            {
                // حاول قص عند أقرب "-" بدل القص العشوائي
                string truncated = slug.Substring(0, maxLength);

                int lastDash = truncated.LastIndexOf('-');
                if (lastDash > 0)
                    slug = truncated.Substring(0, lastDash);
                else
                    slug = truncated;

                slug = slug.Trim('-');
            }

            return slug;
        }

        private static bool IsEnglishLetterOrDigit(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c);
        }

        private static bool IsArabicLetter(char c)
        {
            return (c >= '\u0600' && c <= '\u06FF');
        }

        public static async Task<string> GenerateUniqueSlug<TEntity>(
            string title,
            DBContext context,
            Expression<Func<TEntity, string>> slugPropertySelector
        ) where TEntity : class
        {
            string baseSlug = GenerateSlug(title);
            string slug = baseSlug;
            int count = 1;

            var parameter = Expression.Parameter(typeof(TEntity), "e");

            while (await context.Set<TEntity>().AnyAsync(
                Expression.Lambda<Func<TEntity, bool>>(
                    Expression.Equal(
                        slugPropertySelector.Body,
                        Expression.Constant(slug)
                    ),
                    slugPropertySelector.Parameters
                )))
            {
                // لما يصير تكرار، أضف رقم وتأكد ما تتخطى الطول
                string suffix = $"-{count}";
                if (baseSlug.Length + suffix.Length > 60)
                {
                    baseSlug = baseSlug.Substring(0, 60 - suffix.Length).Trim('-');
                }

                slug = $"{baseSlug}{suffix}";
                count++;
            }

            return slug;
        }
    }
}
