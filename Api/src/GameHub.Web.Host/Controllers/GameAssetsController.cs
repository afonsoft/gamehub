using System;
using System.IO;
using System.Threading.Tasks;
using GameHub.Developer;
using GameHub.Developer.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eaf.Middleware.Web.Controllers;

namespace GameHub.Web.Controllers
{
    /// <summary>
    /// Controller para upload de assets de jogos (thumbnails, hero images).
    /// </summary>
    [Route("api/game-assets")]
    [ApiController]
    public class GameAssetsController : MiddlewareControllerBase
    {
        private readonly IDeveloperGameAppService _developerGameAppService;

        public GameAssetsController(IDeveloperGameAppService developerGameAppService)
        {
            _developerGameAppService = developerGameAppService;
        }

        /// <summary>
        /// Faz upload da thumbnail de um jogo.
        /// </summary>
        /// <param name="gameId">Identificador do jogo.</param>
        /// <param name="file">Imagem (png, jpg, jpeg, webp, gif) até 2 MB.</param>
        /// <returns>URL pública da imagem.</returns>
        [HttpPost("{gameId:guid}/thumbnail")]
        [RequestSizeLimit(2L * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<UploadImageResultDto> UploadThumbnailAsync(Guid gameId, IFormFile file)
        {
            return await UploadAsync(gameId, file, _developerGameAppService.UploadThumbnailAsync);
        }

        /// <summary>
        /// Faz upload da hero image de um jogo.
        /// </summary>
        /// <param name="gameId">Identificador do jogo.</param>
        /// <param name="file">Imagem (png, jpg, jpeg, webp, gif) até 2 MB.</param>
        /// <returns>URL pública da imagem.</returns>
        [HttpPost("{gameId:guid}/hero")]
        [RequestSizeLimit(2L * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<UploadImageResultDto> UploadHeroAsync(Guid gameId, IFormFile file)
        {
            return await UploadAsync(gameId, file, _developerGameAppService.UploadHeroAsync);
        }

        private async Task<UploadImageResultDto> UploadAsync(Guid gameId, IFormFile file, Func<Guid, byte[], string, string, Task<UploadImageResultDto>> upload)
        {
            if (file == null || file.Length == 0)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException("Image file is required.");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return await upload(gameId, stream.ToArray(), file.FileName, file.ContentType);
        }
    }
}
