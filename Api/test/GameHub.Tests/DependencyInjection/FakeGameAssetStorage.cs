using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Builds;
using GameHub.Storage;

namespace GameHub.Tests.DependencyInjection
{
    public class FakeGameAssetStorage : IGameAssetStorage
    {
        public Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken = default)
        {
            var key = $"builds/{package.GameId:N}/{package.BuildId:N}/{package.FileName}";
            var prefix = $"builds/{package.GameId:N}/{package.BuildId:N}/";
            return Task.FromResult(new StoredAsset
            {
                Key = key,
                ETag = "\"etag\"",
                SizeBytes = package.Content?.Length ?? 0,
                Url = $"http://minio/gamehub/{key}",
                PublicBaseUrl = $"http://minio/gamehub/{prefix}",
            });
        }

        public Task<StoredAsset> StoreAssetAsync(AssetUploadInput input, CancellationToken cancellationToken = default)
        {
            var key = $"{input.AssetKind}/{input.GameId:N}/{input.FileName}";
            return Task.FromResult(new StoredAsset
            {
                Key = key,
                ETag = "\"etag\"",
                SizeBytes = input.Content?.Length ?? 0,
                Url = $"http://minio/gamehub/{key}",
                PublicBaseUrl = $"http://minio/gamehub/{input.AssetKind}/{input.GameId:N}/",
            });
        }

        public Task<IReadOnlyList<StoredFile>> ListBuildFilesAsync(Guid gameId, Guid buildId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StoredFile>>(new List<StoredFile>
            {
                new StoredFile
                {
                    Key = $"builds/{gameId:N}/{buildId:N}/index.html",
                    Name = "index.html",
                    SizeBytes = 100,
                    Url = $"http://minio/gamehub/builds/{gameId:N}/{buildId:N}/index.html",
                    ContentType = "text/html",
                }
            });
        }
    }
}
