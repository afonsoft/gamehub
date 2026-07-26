using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Developer.Dto;

namespace GameHub.Builds
{
    public class GameBuildPackageValidator : IGameBuildPackageValidator
    {
        private static readonly string[] BlockedExtensions = { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh" };
        private static readonly string[] DebugFolders = { "node_modules", ".git", "__macosx", "__MACOSX", "test", "tests", "debug", "coverage" };
        private static readonly string[] DebugFileExtensions = { ".map", ".pdb", ".dbg", ".nupkg", ".symbols" };
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        private static readonly Regex ExternalUrlRegex = new(@"https?://[^\s""'<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SplashScreenRegex = new(@"\bsplash\b|intro\.html|loading[_\-]?screen|boot[_\-]?screen", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex OutgoingLinkRegex = new(@"(window\.open\s*\(|location\.href\s*=|<a\s+[^>]*href\s*=\s*[ ""']?https?://)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ConsoleLogRegex = new(@"\bconsole\.(log|warn|error|debug)\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DebuggerStatementRegex = new(@"\bdebugger\s*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const long MaxTextScanBytes = 100L * 1024;
        private static readonly string[] TextExtensions = { ".html", ".htm", ".js", ".json", ".css", ".txt" };

        public async Task<ValidationSummaryDto> ValidateAsync(Stream packageStream, CancellationToken cancellationToken = default)
        {
            var summary = new ValidationSummaryDto();

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
                        await ScanEntryForExternalUrlsAsync(indexEntry, summary, cancellationToken);
                    }

                    summary.HasExternalRequests = summary.ExternalDomains.Any();
                }
            }
            catch (InvalidDataException)
            {
                summary.Errors.Add("Package is not a valid ZIP archive.");
            }

            summary.IsValid = !summary.Errors.Any();
            return summary;
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

            if (TextExtensions.Contains(extension) && entry.Length <= MaxTextScanBytes)
            {
                ScanTextEntryAsync(entry, summary, CancellationToken.None).GetAwaiter().GetResult();
            }

            AddImageOptimizationWarningIfNeeded(entry, summary);
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
            }
        }

        private static async Task ScanEntryForExternalUrlsAsync(ZipArchiveEntry entry, ValidationSummaryDto summary, CancellationToken cancellationToken)
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
                    summary.ExternalDomains.AddRange(matches.Select(m => new Uri(m).Host).Where(h => !string.IsNullOrWhiteSpace(h)).Distinct());
                    summary.Warnings.Add($"External requests found in {entry.FullName}: {string.Join(", ", matches)}");
                }

                if (!content.Contains("viewport", StringComparison.OrdinalIgnoreCase))
                {
                    summary.Warnings.Add($"{entry.FullName} is missing a viewport meta tag; mobile scaling may be broken.");
                }
            }
        }
    }
}
