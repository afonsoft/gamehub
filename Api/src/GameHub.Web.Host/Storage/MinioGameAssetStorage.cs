using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using GameHub.Builds;
using GameHub.Storage;

namespace GameHub.Web.Storage
{
    public class MinioGameAssetStorage : IGameAssetStorage
    {
        private readonly StorageOptions _options;
        private readonly IAmazonS3 _s3Client;

        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = "text/html",
            [".htm"] = "text/html",
            [".js"] = "application/javascript",
            [".mjs"] = "application/javascript",
            [".json"] = "application/json",
            [".css"] = "text/css",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".svg"] = "image/svg+xml",
            [".webp"] = "image/webp",
            [".wasm"] = "application/wasm",
            [".data"] = "application/octet-stream",
            [".unityweb"] = "application/octet-stream",
            [".mem"] = "application/octet-stream",
            [".ogg"] = "audio/ogg",
            [".mp3"] = "audio/mpeg",
            [".mp4"] = "video/mp4",
            [".webm"] = "video/webm",
            [".ttf"] = "font/ttf",
            [".otf"] = "font/otf",
            [".woff"] = "font/woff",
            [".woff2"] = "font/woff2",
            [".eot"] = "application/vnd.ms-fontobject"
        };

        public MinioGameAssetStorage(StorageOptions options)
            : this(options, S3ClientFactory.Create(options))
        {
        }

        public MinioGameAssetStorage(StorageOptions options, IAmazonS3 s3Client)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Minio ??= new MinioStorageOptions();
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        public async Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken = default)
        {
            if (package?.Content == null)
                throw new ArgumentNullException(nameof(package));

            var prefix = $"builds/{package.GameId:N}/{package.BuildId:N}/";
            var packageKey = $"{prefix}{package.FileName}";

            await EnsureBucketExistsAsync(cancellationToken);

            var stream = package.Content;

            // The same stream is read multiple times by the validator and this storage.
            // Guard against a stream that is not at the beginning.
            if (stream.CanSeek && stream.Position != 0)
                stream.Position = 0;

            if (stream.Length == 0)
                throw new InvalidOperationException($"Build package stream is empty (length 0, position {stream.Position}).");

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
            {
                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) || entry.FullName.EndsWith("/"))
                        continue;

                    await UploadEntryAsync(entry, prefix, cancellationToken);
                }
            }

            // Upload the original package as well for audit/reprocessing.
            if (stream.CanSeek)
                stream.Position = 0;

            var packageResponse = await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _options.Minio.Bucket,
                Key = packageKey,
                InputStream = stream,
                ContentType = package.ContentType ?? "application/zip",
                AutoCloseStream = false
            }, cancellationToken);

            return new StoredAsset
            {
                Key = packageKey,
                ETag = packageResponse.ETag,
                SizeBytes = stream.Length,
                Url = BuildPublicUrl(packageKey),
                PublicBaseUrl = BuildPublicUrl(prefix)
            };
        }

        private async Task UploadEntryAsync(ZipArchiveEntry entry, string prefix, CancellationToken cancellationToken)
        {
            var key = $"{prefix}{entry.FullName}";
            var extension = Path.GetExtension(entry.Name);
            var contentType = ContentTypes.TryGetValue(extension, out var value)
                ? value
                : "application/octet-stream";

            await using (var entryStream = entry.Open())
            {
                // AWS SDK PutObject requires a seekable stream to compute the hash/content-length.
                using (var memoryStream = new MemoryStream())
                {
                    await entryStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var request = new PutObjectRequest
                    {
                        BucketName = _options.Minio.Bucket,
                        Key = key,
                        InputStream = memoryStream,
                        ContentType = contentType,
                        AutoCloseStream = false
                    };

                    await _s3Client.PutObjectAsync(request, cancellationToken);
                }
            }
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = _options.Minio.Bucket,
                    UseClientRegion = true
                }, cancellationToken);
            }
            catch (AmazonS3Exception ex) when (
                ex.ErrorCode == "BucketAlreadyExists" ||
                ex.ErrorCode == "BucketAlreadyOwnedByYou" ||
                ex.ErrorCode == "InvalidBucketName")
            {
                // Bucket already exists or name conflict; safe to proceed.
            }
        }

        private string BuildPublicUrl(string key)
        {
            var endpoint = _options.Minio.Endpoint?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new InvalidOperationException("MinIO endpoint is not configured.");

            if (_options.Minio.ForcePathStyle)
                return $"{endpoint}/{_options.Minio.Bucket}/{key}";

            return $"{endpoint}/{key}";
        }

        public async Task<IReadOnlyList<StoredFile>> ListBuildFilesAsync(Guid gameId, Guid buildId, CancellationToken cancellationToken = default)
        {
            var prefix = $"builds/{gameId:N}/{buildId:N}/";
            var files = new List<StoredFile>();
            string continuationToken = null;

            do
            {
                var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _options.Minio.Bucket,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                    MaxKeys = 1000
                }, cancellationToken);

                foreach (var s3Object in response.S3Objects)
                {
                    if (s3Object.Key.EndsWith("/"))
                        continue;

                    var name = s3Object.Key.Length > prefix.Length ? s3Object.Key.Substring(prefix.Length) : s3Object.Key;
                    var extension = Path.GetExtension(name).ToLowerInvariant();
                    ContentTypes.TryGetValue(extension, out var contentType);

                    files.Add(new StoredFile
                    {
                        Key = s3Object.Key,
                        Name = name,
                        SizeBytes = s3Object.Size,
                        LastModified = s3Object.LastModified,
                        Url = BuildPublicUrl(s3Object.Key),
                        ContentType = contentType ?? "application/octet-stream"
                    });
                }

                continuationToken = response.IsTruncated ? response.NextContinuationToken : null;
            } while (!string.IsNullOrEmpty(continuationToken));

            return files;
        }
    }
}
