using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameHub.Contracts
{
    /// <summary>
    /// Resultado de uma verificação de rate limit.
    /// </summary>
    public class RateLimitDecision
    {
        /// <summary>
        /// Verdadeiro quando a requisição é permitida.
        /// </summary>
        public bool Allowed { get; set; }

        /// <summary>
        /// Segundos de espera antes de tentar novamente quando negado.
        /// </summary>
        public long RetryAfterSeconds { get; set; }

        /// <summary>
        /// Timestamp UTC quando a janela atual é reiniciada.
        /// </summary>
        public DateTime ResetAt { get; set; }
    }

    /// <summary>
    /// Contrato que representa uma decisão de rate limit retornada aos clientes.
    /// </summary>
    public class RateLimitContract
    {
        /// <summary>
        /// Verdadeiro quando a requisição é permitida.
        /// </summary>
        public bool Allowed { get; set; }

        /// <summary>
        /// Número atual de requisições dentro da janela.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Número máximo de requisições permitidas na janela.
        /// </summary>
        public long Limit { get; set; }

        /// <summary>
        /// Timestamp UTC quando a janela atual é reiniciada.
        /// </summary>
        public DateTime ResetAt { get; set; }

        /// <summary>
        /// Segundos de espera antes de tentar novamente quando negado.
        /// </summary>
        public long RetryAfterSeconds { get; set; }
    }

    /// <summary>
    /// Gerenciador de rate limit compartilhado entre módulos.
    /// </summary>
    public interface IRateLimitManager
    {
        /// <summary>
        /// Verifica se uma requisição é permitida sob a política e sujeito especificados.
        /// </summary>
        /// <param name="policy">Identificador da política.</param>
        /// <param name="subject">Sujeito tenant-aware da política.</param>
        /// <param name="window">Janela de contagem.</param>
        /// <param name="limit">Quantidade máxima permitida.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>A decisão operacional da política.</returns>
        Task<RateLimitDecision> CheckAsync(
            string policy,
            string subject,
            TimeSpan window,
            int limit,
            CancellationToken cancellationToken = default);
    }
}
