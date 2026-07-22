using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
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
        [Fact]
        public async Task Dado_PackageValido_Quando_Store_Entao_RetornaAssetComUrlCorreta()
        {
            var gameId = Guid.NewGuid();
            var buildId = Guid.NewGuid();
            var content = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

            var s3Client = CreateS3ClientMock();

            var options = new StorageOptions
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

            var storage = new MinioGameAssetStorage(options, s3Client);
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
            asset.SizeBytes.ShouldBe(3);
            asset.Url.ShouldBe($"http://localhost:9000/gamehub-builds/builds/{gameId:N}/{buildId:N}/build.zip");

            await s3Client.Received(1).PutObjectAsync(
                Arg.Is<PutObjectRequest>(r =>
                    r.BucketName == "gamehub-builds" &&
                    r.Key == $"builds/{gameId:N}/{buildId:N}/build.zip" &&
                    r.ContentType == "application/zip"),
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

            var options = new StorageOptions
            {
                Minio = new MinioStorageOptions
                {
                    Endpoint = "http://localhost:9000",
                    Bucket = "gamehub-builds"
                }
            };

            var storage = new MinioGameAssetStorage(options, s3Client);

            var asset = await storage.StoreAsync(new GameBuildPackage
            {
                GameId = Guid.NewGuid(),
                BuildId = Guid.NewGuid(),
                FileName = "build.zip",
                Content = new MemoryStream(new byte[] { 0x01 })
            });

            asset.ShouldNotBeNull();
            await s3Client.Received(1).PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_PackageNulo_Quando_Store_Entao_LancaArgumentNullException()
        {
            var s3Client = Substitute.For<IAmazonS3>();
            var storage = new MinioGameAssetStorage(new StorageOptions(), s3Client);

            await Should.ThrowAsync<ArgumentNullException>(() => storage.StoreAsync(null));
        }

        private static IAmazonS3 CreateS3ClientMock()
        {
            var s3Client = Substitute.For<IAmazonS3>();

            s3Client.PutBucketAsync(Arg.Any<PutBucketRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutBucketResponse());
            s3Client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PutObjectResponse { ETag = "\"abc123\"" });

            return s3Client;
        }
    }
}
