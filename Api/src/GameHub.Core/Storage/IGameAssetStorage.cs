using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameHub.Builds;

namespace GameHub.Storage
{
    public interface IGameAssetStorage
    {
        Task<StoredAsset> StoreAsync(GameBuildPackage package, CancellationToken cancellationToken = default);
    }
}
