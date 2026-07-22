using System;
using System.IO;

namespace GameHub.Builds
{
    public class GameBuildPackage
    {
        public Guid GameId { get; set; }
        public Guid BuildId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream Content { get; set; }
    }
}
