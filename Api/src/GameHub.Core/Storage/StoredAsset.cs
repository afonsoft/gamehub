using System;

namespace GameHub.Storage
{
    public class StoredAsset
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public string ETag { get; set; }
        public long SizeBytes { get; set; }
    }
}
