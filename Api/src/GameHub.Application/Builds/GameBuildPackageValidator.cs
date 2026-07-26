using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using GameHub.Developer.Dto;
using GameHub.Moderation;
using Microsoft.EntityFrameworkCore;

namespace GameHub.Builds
{
    public class GameBuildPackageValidator : IGameBuildPackageValidator
    {
        private readonly IRepository<ExternalResourceExemption, Guid> _exemptionRepository;

        public GameBuildPackageValidator()
        {
        }

        public GameBuildPackageValidator(IRepository<ExternalResourceExemption, Guid> exemptionRepository)
        {
            _exemptionRepository = exemptionRepository;
        }

        private static readonly string[] BlockedExtensions = { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh" };
        private static readonly string[] DebugFolders = { "node_modules", ".git", "__macosx", "__MACOSX", "test", "tests", "debug", "coverage" };
        private static readonly string[] DebugFileExtensions = { ".map", ".pdb", ".dbg", ".nupkg", ".symbols" };
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        private static readonly string[] ThumbnailFileNames = { "thumbnail", "hero", "cover" };
        private const long MaxThumbnailSizeBytes = 2L * 1024 * 1024;
        private const int MinThumbnailWidth = 640;
        private const int MinThumbnailHeight = 360;
        private const double MinAspectRatio = 1.7;
        private const double MaxAspectRatio = 1.8;
        private static readonly Regex ExternalUrlRegex = new(@"https?://[^\s""'<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SplashScreenRegex = new(@"\bsplash\b|intro\.html|loading[_\-]?screen|boot[_\-]?screen", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex OutgoingLinkRegex = new(@"(window\.open\s*\(|location\.href\s*=|<a\s+[^>]*href\s*=\s*[ ""']?https?://)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ConsoleLogRegex = new(@"\bconsole\.(log|warn|error|debug)\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DebuggerStatementRegex = new(@"\bdebugger\s*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex InAppPurchaseRegex = new(@"\b(buy|purchase|premium|remove ads|no ads|disable ads|iap|in-app purchase|in app purchase|shop|store|checkout|payment)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ThumbnailTextOverlayRegex = new(@"\b(text|title|overlay|banner|label|caption|with[_-]?text)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const long MaxTextScanBytes = 100L * 1024;
        private static readonly string[] TextExtensions = { ".html", ".htm", ".js", ".json", ".css", ".txt" };

        public async Task<ValidationSummaryDto> ValidateAsync(Stream packageStream, Guid? gameId = null, CancellationToken cancellationToken = default)
        {
            var summary = new ValidationSummaryDto();
            var approvedDomains = await GetApprovedDomainsAsync(gameId, cancellationToken);

            if (packageStream == null || packageStream.Length == 0)
            {
                summary.Errors.Add("Package stream is empty.");
                return summary;
            }

            summary.PackageSizeBytes = packageStream.Length;

            if (packageStream.Length > GameHubConsts.MaxBuildPackageSizeBytes)
            {
                summary.Errors.Add($"Package exceeds maximum size of {GameHubConsts.MaxBuildPackageSizeBytes} bytes.");
                return summary;
            }

            if (packageStream.Length > GameHubConsts.BuildPackageWarningSizeBytes)
            {
                summary.Warnings.Add($"Package is larger than {GameHubConsts.BuildPackageWarningSizeBytes / (1024 * 1024)} MB; consider optimizing assets.");
            }

            packageStream.Position = 0;
            using (var sha = SHA256.Create())
            {
                var hash = await sha.ComputeHashAsync(packageStream, cancellationToken);
                summary.HashSha256 = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }

            packageStream.Position = 0;
            try
            {
                using (var zip = new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: true))
                {
                    var entries = zip.Entries;
                    var indexEntry = entries.FirstOrDefault(e => e.FullName.Equals("index.html", StringComparison.OrdinalIgnoreCase));
                    summary.HasIndexHtml = indexEntry != null;
                    summary.IndexHtmlPath = summary.HasIndexHtml ? indexEntry.FullName : string.Empty;

                    if (!summary.HasIndexHtml)
                    {
                        summary.Errors.Add("Package must contain an index.html at the root.");
                    }

                    foreach (var entry in entries)
                    {
                        ValidateEntry(entry, summary);
                    }

                    if (summary.HasIndexHtml)
                    {
                        await ScanEntryForExternalUrlsAsync(indexEntry, summary, approvedDomains, cancellationToken);
                    }

                    summary.HasExternalRequests = summary.ExternalDomains.Any(d => !approvedDomains.Contains(d, StringComparer.OrdinalIgnoreCase));
                }
            }
            catch (InvalidDataException)
            {
                summary.Errors.Add("Package is not a valid ZIP archive.");
            }

            summary.IsValid = !summary.Errors.Any();
            summary.QualityScore = ComputeQualityScore(summary);
            return summary;
        }

        private static int ComputeQualityScore(ValidationSummaryDto summary)
        {
            var score = 100;
            score -= summary.Errors.Count * 10;
            score -= summary.Warnings.Count * 2;
            score -= summary.ImageOptimizationWarnings.Count * 3;

            if (summary.HasExternalRequests)
            {
                score -= 5;
            }

            if (!summary.HasIndexHtml)
            {
                score -= 30;
            }

            return Math.Max(0, score);
        }

        private async Task<List<string>> GetApprovedDomainsAsync(Guid? gameId, CancellationToken cancellationToken)
        {
            if (!gameId.HasValue || _exemptionRepository == null)
            {
                return new List<string>();
            }

            var exemptions = await _exemptionRepository.GetAll()
                .Where(e => e.GameId == gameId.Value && e.Status == ExternalResourceExemptionStatus.Approved)
                .Select(e => e.Domain)
                .ToListAsync(cancellationToken);

            return exemptions;
        }

        private static void ValidateEntry(ZipArchiveEntry entry, ValidationSummaryDto summary)
        {
            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
            if (BlockedExtensions.Contains(extension))
            {
                summary.Errors.Add($"Package contains blocked executable file: {entry.FullName}");
            }

            if (DebugFileExtensions.Contains(extension))
            {
                summary.Warnings.Add($"Package contains debug artifact: {entry.FullName}. Remove source maps and symbols before publishing.");
            }

            if (entry.Length > GameHubConsts.LargeFileWarningSizeBytes)
            {
                summary.Warnings.Add($"Large file detected: {entry.FullName} ({entry.Length / 1024} KB).");
            }

            var pathSegments = entry.FullName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Any(s => DebugFolders.Contains(s, StringComparer.OrdinalIgnoreCase)))
            {
                summary.Warnings.Add($"Package contains development/debug artifact: {entry.FullName}");
            }

            if (SplashScreenRegex.IsMatch(entry.FullName))
            {
                summary.Warnings.Add($"Possible splash screen file detected: {entry.FullName}. Remove splash screens before publishing.");
            }

            CheckFilenameQuality(entry, summary);

            if (TextExtensions.Contains(extension) && entry.Length <= MaxTextScanBytes)
            {
                ScanTextEntryAsync(entry, summary, CancellationToken.None).GetAwaiter().GetResult();
            }

            AddImageOptimizationWarningIfNeeded(entry, summary);
            ValidateThumbnailDimensions(entry, summary);
        }

        private static void ValidateThumbnailDimensions(ZipArchiveEntry entry, ValidationSummaryDto summary)
        {
            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
            if (!ImageExtensions.Contains(extension))
            {
                return;
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entry.Name).ToLowerInvariant();
            if (!ThumbnailFileNames.Any(t => fileNameWithoutExtension.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (entry.Length > MaxThumbnailSizeBytes)
            {
                summary.Warnings.Add($"Thumbnail '{entry.FullName}' exceeds {MaxThumbnailSizeBytes / (1024 * 1024)} MB.");
            }

            using (var stream = entry.Open())
            {
                var dimensions = ImageHeaderAnalyzer.TryGetDimensions(stream);
                if (!dimensions.HasValue)
                {
                    summary.Warnings.Add($"Could not read dimensions of '{entry.FullName}'. Ensure it is a valid image.");
                    return;
                }

                var (width, height) = dimensions.Value;
                if (width < MinThumbnailWidth || height < MinThumbnailHeight)
                {
                    summary.Warnings.Add($"Thumbnail '{entry.FullName}' is {width}x{height}; minimum recommended is {MinThumbnailWidth}x{MinThumbnailHeight}.");
                }

                var ratio = (double)width / height;
                if (ratio < MinAspectRatio || ratio > MaxAspectRatio)
                {
                    summary.Warnings.Add($"Thumbnail '{entry.FullName}' aspect ratio is {ratio:F2}; recommended 16:9 (~1.78).");
                }
            }

            if (extension != ".webp")
            {
                summary.Warnings.Add($"Thumbnail '{entry.FullName}' is not WebP; consider converting for better compression.");
            }
        }

        private static void CheckFilenameQuality(ZipArchiveEntry entry, ValidationSummaryDto summary)
        {
            var profanityFilter = new ProfanityFilter();
            if (profanityFilter.ContainsProfanity(entry.FullName))
            {
                summary.Errors.Add($"Profanity detected in filename: {entry.FullName}.");
            }

            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
            if (ImageExtensions.Contains(extension) && ThumbnailTextOverlayRegex.IsMatch(entry.FullName))
            {
                summary.Warnings.Add($"Image filename '{entry.FullName}' suggests text overlay. Thumbnails should not contain text.");
            }
        }

        private static void AddImageOptimizationWarningIfNeeded(ZipArchiveEntry entry, ValidationSummaryDto summary)
        {
            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
            if (!ImageExtensions.Contains(extension) || entry.Length <= GameHubConsts.ImageOptimizationWarningSizeBytes)
            {
                return;
            }

            var savingsRatio = extension == ".webp" ? 0.15 : extension == ".gif" ? 0.40 : 0.70;
            var estimatedSavings = (long)(entry.Length * savingsRatio);
            var recommendation = extension == ".webp"
                ? "Image is already WebP but still large; consider reducing dimensions or quality."
                : $"Consider converting {extension} to WebP or compressing to reduce size.";

            summary.ImageOptimizationWarnings.Add(new GameHub.Developer.Dto.ImageOptimizationWarningDto
            {
                Path = entry.FullName,
                CurrentSizeBytes = entry.Length,
                EstimatedSavingsBytes = estimatedSavings,
                Recommendation = recommendation
            });

            summary.Warnings.Add($"Image asset '{entry.FullName}' ({entry.Length / 1024} KB) can be optimized. {recommendation}");
        }

        private static async Task ScanTextEntryAsync(ZipArchiveEntry entry, ValidationSummaryDto summary, CancellationToken cancellationToken)
        {
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var content = await reader.ReadToEndAsync(cancellationToken);

                if (OutgoingLinkRegex.IsMatch(content))
                {
                    summary.Warnings.Add($"Outgoing link or navigation detected in {entry.FullName}. Remove external links before publishing.");
                }

                if (ConsoleLogRegex.IsMatch(content))
                {
                    summary.Warnings.Add($"Console logging detected in {entry.FullName}. Remove debug statements before publishing.");
                }

                if (DebuggerStatementRegex.IsMatch(content))
                {
                    summary.Warnings.Add($"Debugger statement detected in {entry.FullName}. Remove debug breakpoints before publishing.");
                }

                if (InAppPurchaseRegex.IsMatch(content))
                {
                    summary.Warnings.Add($"Possible in-app purchase / payment UI terms detected in {entry.FullName}. Poki handles monetization.");
                }

                var profanityFilter = new ProfanityFilter();
                if (profanityFilter.ContainsProfanity(content))
                {
                    summary.Errors.Add($"Profanity detected in {entry.FullName}. Remove offensive language before publishing.");
                }
            }
        }

        private static async Task ScanEntryForExternalUrlsAsync(ZipArchiveEntry entry, ValidationSummaryDto summary, List<string> approvedDomains, CancellationToken cancellationToken)
        {
            if (entry.Length > MaxTextScanBytes)
            {
                return;
            }

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var content = await reader.ReadToEndAsync(cancellationToken);
                var matches = ExternalUrlRegex.Matches(content)
                    .Select(m => m.Value)
                    .Distinct()
                    .Take(5)
                    .ToList();

                if (matches.Any())
                {
                    var hosts = matches
                        .Select(m => new Uri(m).Host)
                        .Where(h => !string.IsNullOrWhiteSpace(h))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    summary.ExternalDomains.AddRange(hosts);
                    var unapproved = hosts.Where(h => !approvedDomains.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();
                    if (unapproved.Any())
                    {
                        summary.Warnings.Add($"External requests found in {entry.FullName}: {string.Join(", ", unapproved)}. Request an exemption if the domain is required.");
                    }
                }

                if (!content.Contains("viewport", StringComparison.OrdinalIgnoreCase))
                {
                    summary.Warnings.Add($"{entry.FullName} is missing a viewport meta tag; mobile scaling may be broken.");
                }
            }
        }
    }
}
