using System;

namespace GameHub.Admin.Dto;

/// <summary>
/// Alerta de saúde de um jogo detectado no dashboard administrativo.
/// </summary>
public class AdminHealthAlertDto
{
    /// <summary>Identificador do jogo.</summary>
    public Guid GameId { get; set; }

    /// <summary>Título do jogo.</summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>Motivo do alerta.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Severidade: Info, Warning, Critical.</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>Valor da métrica que gerou o alerta.</summary>
    public double MetricValue { get; set; }
}
