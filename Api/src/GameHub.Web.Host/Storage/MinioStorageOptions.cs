namespace GameHub.Web.Storage
{
    public class MinioStorageOptions
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Bucket { get; set; } = "gamehub-builds";
        public string Region { get; set; } = "us-east-1";
        public bool ForcePathStyle { get; set; } = true;
    }

    public class StorageOptions
    {
        public string Provider { get; set; } = "MinIO";
        public MinioStorageOptions Minio { get; set; } = new MinioStorageOptions();
    }
}
