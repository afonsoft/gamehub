using System;
using System.Text;
using System.Threading.Tasks;
using GameHub.Developer;
using GameHub.Developer.Dto;
using GameHub.Gameplay;
using GameHub.Gameplay.Dto;
using Microsoft.AspNetCore.Mvc;
using Eaf.Middleware.Web.Controllers;

namespace GameHub.Web.Controllers
{
    /// <summary>
    /// API endpoints for downloading CSV reports from the portal.
    /// </summary>
    [Route("api/exports")]
    [ApiController]
    public class ExportController : MiddlewareControllerBase
    {
        private readonly IDeveloperEarningsAppService _developerEarningsAppService;
        private readonly IGameMetricsAppService _gameMetricsAppService;

        public ExportController(
            IDeveloperEarningsAppService developerEarningsAppService,
            IGameMetricsAppService gameMetricsAppService)
        {
            _developerEarningsAppService = developerEarningsAppService;
            _gameMetricsAppService = gameMetricsAppService;
        }

        /// <summary>
        /// Download the earnings report as CSV for the current developer.
        /// </summary>
        /// <param name="from">Optional period start date.</param>
        /// <param name="to">Optional period end date.</param>
        /// <returns>CSV file with estimated earnings.</returns>
        [HttpGet("earnings")]
        public async Task<IActionResult> EarningsCsv([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var export = await _developerEarningsAppService.ExportCsvAsync(new GetDeveloperEarningsInput
            {
                From = from,
                To = to
            });

            return File(Encoding.UTF8.GetBytes(export.Content), export.ContentType, export.FileName);
        }

        /// <summary>
        /// Download the game metrics report as CSV.
        /// </summary>
        /// <param name="gameId">Game identifier.</param>
        /// <param name="from">Optional period start date.</param>
        /// <param name="to">Optional period end date.</param>
        /// <returns>CSV file with metrics.</returns>
        [HttpGet("metrics/{gameId:guid}")]
        public async Task<IActionResult> MetricsCsv(Guid gameId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var export = await _gameMetricsAppService.ExportCsvAsync(gameId, new GameMetricsFilter
            {
                From = from,
                To = to
            });

            return File(Encoding.UTF8.GetBytes(export.Content), export.ContentType, export.FileName);
        }
    }
}
