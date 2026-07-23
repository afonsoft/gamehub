using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace GameHub.Web.Storage
{
    public static class S3ClientFactory
    {
        public static IAmazonS3 Create(StorageOptions options)
        {
            var minio = options?.Minio ?? new MinioStorageOptions();
            var s3Config = new AmazonS3Config
            {
                ServiceURL = minio.Endpoint,
                ForcePathStyle = minio.ForcePathStyle,
            };

            if (string.IsNullOrWhiteSpace(minio.Endpoint))
            {
                s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(minio.Region ?? "us-east-1");
            }
            else
            {
                s3Config.AuthenticationRegion = minio.Region ?? "us-east-1";
            }

            return new AmazonS3Client(minio.AccessKey, minio.SecretKey, s3Config);
        }
    }
}
