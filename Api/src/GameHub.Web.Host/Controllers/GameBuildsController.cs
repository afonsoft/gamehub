using System;
using System.IO;
using System.Threading.Tasks;
using GameHub.Builds;
using GameHub.Developer.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Eaf.Middleware.Web.Controllers;

namespace GameHub.Web.Controllers
{
    /// <summary>
    /// Controller para upload de builds de jogos.
    /// </summary>
    [Route("api/game-builds")]
    [ApiController]
    public class GameBuildsController : MiddlewareControllerBase
    {
        private readonly IGameBuildAppService _gameBuildAppService;

        public GameBuildsController(IGameBuildAppService gameBuildAppService)
        {
            _gameBuildAppService = gameBuildAppService;
        }

        /// <summary>
        /// Faz upload de um pacote de build HTML5/WebGL para um jogo.
        /// </summary>
        /// <param name="gameId">Identificador do jogo.</param>
        /// <param name="file">Arquivo zip do build.</param>
        /// <returns>Resultado do upload e validação.</returns>
        [HttpPost("{gameId:guid}/upload")]
        [RequestSizeLimit(GameHubConsts.MaxBuildPackageSizeBytes)]
        [Consumes("multipart/form-data")]
        public async Task<UploadGameBuildResultDto> UploadAsync(Guid gameId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new System.ComponentModel.DataAnnotations.ValidationException("Arquivo de build é obrigatório.");
            }

            using var stream = file.OpenReadStream();
            return await _gameBuildAppService.UploadBuildAsync(gameId, stream, file.FileName, file.ContentType);
        }
    }
}
