using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using GameHub.Builds;
using GameHub.Web.Storage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class MinioGameAssetStorage_Tests
    {
        private static readonly StorageOptions Options = new StorageOptions
        {
            Provider = "MinIO",
            Minio = new MinioStorageOptions
            {
                Endpoint = "http://localhost:9000",
                AccessKey = "gamehub_user",
                SecretKey = "secret",
                Bucket = "gamehub-builds",
                Region = "us-east-1",
                ForcePathStyle = true
            }
        };

        private static IAmazonS3 CreateS3ClientMock()
        {
            var s3Client = Substitute.For<IAmazonS3>();

            s3Client.PutBucketAsync(Arg.Any<PutBucketRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutBucketResponse());
            s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutObjectResponse { ETag = "\"abc123\"" });

            return s3Client;
        }

        private static Stream CreateZipStream(Dictionary<string, string> entries)
        {
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var entry in entries)
                {
                    var zipEntry = zip.CreateEntry(entry.Key);
                    using (var entryStream = zipEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write(entry.Value);
                    }
                }
            }
            stream.Position = 0;
            return stream;
        }

        [Fact]
        public async Task Dado_PackageValido_Quando_Store_Entao_RetornaAssetComUrlCorreta()
        {
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();
            var content = CreateZipStream(new Dictionary<string, string>
            {
                { "index.html", "<html></html>" },
                { "game.js", "console.log('game');" }
            });

            var s3Client = CreateS3ClientMock();
            var storage = new MinioGameAssetStorage(Options, s3Client);

            var package = new GameBuildPackage
            {
                GameId = gameId,
                BuildId = buildId,
                FileName = "build.zip",
                ContentType = "application/zip",
                Content = content
            };

            var asset = await storage.StoreAsync(package);

            asset.ShouldNotBeNull();
            asset.Key.ShouldBe($"builds/{gameId:N}/{buildId:N}/build.zip");
            asset.ETag.ShouldBe("\"abc123\"");
            asset.Url.ShouldBe($"http://localhost:9000/gamehub-builds/builds/{gameId:N}/{buildId:N}/build.zip");
            asset.PublicBaseUrl.ShouldBe($"http://localhost:9000/gamehub-builds/builds/{gameId:N}/{buildId:N}/");

            var putObjectCalls = s3Client.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == nameof(IAmazonS3.PutObjectAsync))
                .ToList();

            // 3 uploads: index.html, game.js and the original package.
            putObjectCalls.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Dado_PackageComIndexHtml_Quando_Store_Entao_PublicBaseUrlApontaParaPrefixo()
        {
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();
            var content = CreateZipStream(new Dictionary<string, string>
            {
                { "index.html", "<html></html>" }
            });

            var s3Client = CreateS3ClientMock();
            var storage = new MinioGameAssetStorage(Options, s3Client);

            var asset = await storage.StoreAsync(new GameBuildPackage
            {
                GameId = gameId,
                BuildId = buildId,
                FileName = "build.zip",
                ContentType = "application/zip",
                Content = content
            });

            var expectedPrefix = $"http://localhost:9000/gamehub-builds/builds/{gameId:N}/{buildId:N}/";
            asset.PublicBaseUrl.ShouldBe(expectedPrefix);

            await s3Client.Received(1).PutObjectAsync(
                Arg.Is<PutObjectRequest>(r =>
                    r.BucketName == "gamehub-builds" &&
                    r.Key == $"builds/{gameId:N}/{buildId:N}/index.html" &&
                    r.ContentType == "text/html"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_BucketJaExistente_Quando_Store_Entao_IgnoraErroEContinuaUpload()
        {
            var s3Client = Substitute.For<IAmazonS3>();
            s3Client.PutBucketAsync(Arg.Any<PutBucketRequest>(), Arg.Any<CancellationToken>())
                .Throws(new AmazonS3Exception("BucketAlreadyOwnedByYou")
                {
                    ErrorCode = "BucketAlreadyOwnedByYou"
                });
            s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutObjectResponse { ETag = "\"etag\"" });

            var storage = new MinioGameAssetStorage(Options, s3Client);

            var content = CreateZipStream(new Dictionary<string, string>
            {
                { "index.html", "<html></html>" }
            });

            var asset = await storage.StoreAsync(new GameBuildPackage
            {
                GameId = Guid.NewGuid(),
                BuildId = Guid.NewGuid(),
                FileName = "build.zip",
                Content = content
            });

            asset.ShouldNotBeNull();
            var putObjectCalls = s3Client.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name == nameof(IAmazonS3.PutObjectAsync))
                .ToList();
            putObjectCalls.Count.ShouldBe(2); // index.html + original package
        }

        [Fact]
        public async Task Dado_PackageNulo_Quando_Store_Entao_LancaArgumentNullException()
        {
            var s3Client = Substitute.For<IAmazonS3>();
            var storage = new MinioGameAssetStorage(new StorageOptions(), s3Client);

            await Should.ThrowAsync<ArgumentNullException>(() => storage.StoreAsync(null));
        }
    }
}
