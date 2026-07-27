namespace GameHub.Contracts
{
    /// <summary>
    /// Envelope de erro seguro retornado pelas APIs EAF para clientes SDK.
    /// </summary>
    public sealed class PublicErrorContract
    {
        /// <summary>
        /// Código estável e legível por máquina.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Mensagem de erro legível por humanos.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Indica se o cliente pode repetir a mesma requisição.
        /// </summary>
        public bool Retryable { get; set; }

        /// <summary>
        /// Identificador de correlação para rastreamento distribuído.
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
