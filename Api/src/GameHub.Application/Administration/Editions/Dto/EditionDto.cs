using Abp.Application.Services.Dto;
using System;

namespace GameHub.Administration.Editions.Dto
{
    /// <summary>
    /// DTO para representar uma Edition.
    /// </summary>
    public class EditionDto : EntityDto<int>
    {
        /// <summary>
        /// Nome exibido da edição.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Identifica se a edição é paga.
        /// </summary>
        public bool IsFree { get; set; }

        /// <summary>
        /// Valor da assinatura mensal.
        /// </summary>
        public decimal? MonthlyPrice { get; set; }

        /// <summary>
        /// Valor da assinatura anual.
        /// </summary>
        public decimal? AnnualPrice { get; set; }

        /// <summary>
        /// Tempo de trial em dias.
        /// </summary>
        public int? TrialDayCount { get; set; }

        /// <summary>
        /// Período de pagamento padrão.
        /// </summary>
        public int? WaitingDayAfterExpire { get; set; }

        /// <summary>
        /// Identificador da edição de expiração.
        /// </summary>
        public int? ExpiringEditionId { get; set; }
    }
}
