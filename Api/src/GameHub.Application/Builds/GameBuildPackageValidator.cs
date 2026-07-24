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
        private static readonly Regex ExternalUrlRegex = new(@"https?://[^\s""'<>]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const long MaxTextScanBytes = 100L * 1024;

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

            if (entry.Length > GameHubConsts.LargeFileWarningSizeBytes)
            {
                summary.Warnings.Add($"Large file detected: {entry.FullName} ({entry.Length / 1024} KB).");
            }

            var pathSegments = entry.FullName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Any(s => DebugFolders.Contains(s, StringComparer.OrdinalIgnoreCase)))
            {
                summary.Warnings.Add($"Package contains development/debug artifact: {entry.FullName}");
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
