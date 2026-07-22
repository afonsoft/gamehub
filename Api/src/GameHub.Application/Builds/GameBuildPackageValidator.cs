using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Developer.Dto;

namespace GameHub.Builds
{
    public class GameBuildPackageValidator : IGameBuildPackageValidator
    {
        private static readonly string[] BlockedExtensions = { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh" };

        public async Task<ValidationSummaryDto> ValidateAsync(Stream packageStream, CancellationToken cancellationToken = default)
        {
            var summary = new ValidationSummaryDto();

            if (packageStream == null || packageStream.Length == 0)
            {
                summary.Errors.Add("Package stream is empty.");
                return summary;
            }

            if (packageStream.Length > GameHubConsts.MaxBuildPackageSizeBytes)
            {
                summary.Errors.Add($"Package exceeds maximum size of {GameHubConsts.MaxBuildPackageSizeBytes} bytes.");
            }

            summary.SizeBytes = packageStream.Length;

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
                        var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
                        if (BlockedExtensions.Contains(extension))
                        {
                            summary.Errors.Add($"Package contains blocked executable file: {entry.FullName}");
                        }
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
    }
}
