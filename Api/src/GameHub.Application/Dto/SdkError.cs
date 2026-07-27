namespace GameHub.Dto;

/// <summary>
/// Envelope de erro público e seguro para consumidores do GameHub SDK.
/// </summary>
public class SdkError
{
    /// <summary>
    /// Código estável do erro para tratamento programático.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem localizada e segura para exibição na interface.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a operação pode ser repetida automaticamente.
    /// </summary>
    public bool Retryable { get; set; }

    /// <summary>
    /// Identificador de correlação para rastreamento nos logs.
    /// </summary>
    public string CorrelationId { get; set; }
}
