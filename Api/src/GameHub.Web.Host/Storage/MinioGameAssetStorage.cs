using System;
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

            var key = $"builds/{package.GameId:N}/{package.BuildId:N}/{package.FileName}";

            await EnsureBucketExistsAsync(cancellationToken);

            var request = new PutObjectRequest
            {
                BucketName = _options.Minio.Bucket,
                Key = key,
                InputStream = package.Content,
                ContentType = package.ContentType ?? "application/octet-stream",
                AutoCloseStream = false
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            return new StoredAsset
            {
                Key = key,
                ETag = response.ETag,
                SizeBytes = package.Content.Length,
                Url = BuildPublicUrl(key)
            };
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
    }
}
