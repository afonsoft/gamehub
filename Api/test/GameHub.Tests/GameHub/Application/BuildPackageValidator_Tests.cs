using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using GameHub.Builds;
using Shouldly;
using Xunit;

namespace GameHub.Tests.GameHub.Application
{
    public class BuildPackageValidator_Tests
    {
        [Fact]
        public async Task Dado_ZipComIndexHtml_Quando_Validar_Entao_DeveSerValido()
        {
            var stream = CreateZip(new[] { "index.html" });
            var validator = new GameBuildPackageValidator();

            var result = await validator.ValidateAsync(stream);

            result.IsValid.ShouldBeTrue();
            result.HasIndexHtml.ShouldBeTrue();
        }

        [Fact]
        public async Task Dado_ZipSemIndexHtml_Quando_Validar_Entao_DeveSerInvalido()
        {
            var stream = CreateZip(new[] { "game.js" });
            var validator = new GameBuildPackageValidator();

            var result = await validator.ValidateAsync(stream);

            result.IsValid.ShouldBeFalse();
            result.HasIndexHtml.ShouldBeFalse();
        }

        [Fact]
        public async Task Dado_ZipComExecutavel_Quando_Validar_Entao_DeveSerInvalido()
        {
            var stream = CreateZip(new[] { "index.html", "malware.exe" });
            var validator = new GameBuildPackageValidator();

            var result = await validator.ValidateAsync(stream);

            result.IsValid.ShouldBeFalse();
        }

        private static MemoryStream CreateZip(string[] entries)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var entry in entries)
                {
                    var zipEntry = archive.CreateEntry(entry);
                    using (var writer = new StreamWriter(zipEntry.Open(), Encoding.UTF8))
                    {
                        writer.Write("content");
                    }
                }
            }

            stream.Position = 0;
            return stream;
        }
    }
}
