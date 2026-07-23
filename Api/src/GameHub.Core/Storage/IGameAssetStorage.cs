using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Builds;

using System;
using System.Collections.Generic;

namespace GameHub.Storage
{
    public interface IGameAssetStorage
    {
        Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken = default);

        Task<StoredAsset> StoreAssetAsync(AssetUploadInput input, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StoredFile>> ListBuildFilesAsync(Guid gameId, Guid buildId, CancellationToken cancellationToken = default);
    }
}
