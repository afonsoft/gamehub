using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Monetization.Dto
{
    /// <summary>
    /// Input para solicitar um break de anúncio.
    /// </summary>
    public class RequestAdBreakInput
    {
        /// <summary>Jogo que solicitou o anúncio.</summary>
        [Required]
        public Guid GameId { get; set; }
    }
}
