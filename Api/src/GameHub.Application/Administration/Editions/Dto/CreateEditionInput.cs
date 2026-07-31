using System;
using System.ComponentModel.DataAnnotations;

namespace GameHub.Administration.Editions.Dto
{
    /// <summary>
    /// Entrada para criação de uma Edition.
    /// </summary>
    public class CreateEditionInput
    {
        /// <summary>
        /// Nome exibido da edição.
        /// </summary>
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Identifica se a edição é gratuita.
        /// </summary>
        public bool IsFree { get; set; }

        /// <summary>
        /// Valor mensal da assinatura.
        /// </summary>
        public decimal? MonthlyPrice { get; set; }

        /// <summary>
        /// Valor anual da assinatura.
        /// </summary>
        public decimal? AnnualPrice { get; set; }

        /// <summary>
        /// Dias de trial.
        /// </summary>
        public int? TrialDayCount { get; set; }

        /// <summary>
        /// Dias de carência após expiração.
        /// </summary>
        public int? WaitingDayAfterExpire { get; set; }

        /// <summary>
        /// Identificador da edição de expiração.
        /// </summary>
        public int? ExpiringEditionId { get; set; }
    }
}
