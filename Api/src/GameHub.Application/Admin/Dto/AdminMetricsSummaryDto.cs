using System;
using System.Collections.Generic;

namespace GameHub.Admin.Dto;

/// <summary>
/// Métricas agregadas de jogadores e jogos para o dashboard administrativo.
/// </summary>
public class AdminMetricsSummaryDto
{
    /// <summary>Data de início do período.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Data de fim do período.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Total de plays no período.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Usuários ativos hoje (DAU).</summary>
    public long DailyActiveUsers { get; set; }

    /// <summary>Usuários ativos nos últimos 30 dias (MAU).</summary>
    public long MonthlyActiveUsers { get; set; }

    /// <summary>Duração média das sessões em segundos.</summary>
    public double AverageSessionDurationSeconds { get; set; }

    /// <summary>Duração mediana das sessões em segundos.</summary>
    public double MedianSessionDurationSeconds { get; set; }

    /// <summary>Taxa de drop-off no primeiro minuto (0 a 1).</summary>
    public double OnboardingDropOffRate { get; set; }

    /// <summary>Taxa de conversão de carregamento (0 a 1).</summary>
    public double LoadConversionRate { get; set; }

    /// <summary>Taxa de erro por eventos de gameplay (0 a 1).</summary>
    public double ErrorRate { get; set; }

    /// <summary>Distribuição por dispositivo.</summary>
    public List<MetricDistributionItemDto> Devices { get; set; } = new();

    /// <summary>Distribuição por país.</summary>
    public List<MetricDistributionItemDto> Countries { get; set; } = new();

    /// <summary>Distribuição por navegador.</summary>
    public List<MetricDistributionItemDto> Browsers { get; set; } = new();

    /// <summary>FPS médio agregado das sessões.</summary>
    public double? AverageFps { get; set; }

    /// <summary>FPS mínimo agregado das sessões.</summary>
    public double? MinimumFps { get; set; }

    /// <summary>Sessões com FPS aceitável (>= 30).</summary>
    public long FpsAcceptableSessions { get; set; }

    /// <summary>Total de sessões que reportaram FPS.</summary>
    public long FpsTotalSessions { get; set; }

    /// <summary>Distribuição de FPS por dispositivo.</summary>
    public List<MetricFpsDistributionItemDto> FpsByDevice { get; set; } = new();
}

/// <summary>Item de distribuição de uma métrica por dimensão.</summary>
public class MetricDistributionItemDto
{
    /// <summary>Nome da dimensão.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Valor absoluto.</summary>
    public long Count { get; set; }

    /// <summary>Percentual sobre o total (0 a 1).</summary>
    public double Percentage { get; set; }
}

/// <summary>Distribuição de FPS por dispositivo.</summary>
public class MetricFpsDistributionItemDto
{
    /// <summary>Dispositivo.</summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>Total de sessões com FPS reportado.</summary>
    public long TotalSessions { get; set; }

    /// <summary>Sessões com FPS aceitável (>= 30).</summary>
    public long AcceptableSessions { get; set; }

    /// <summary>Percentual de sessões aceitáveis (0 a 1).</summary>
    public double AcceptablePercentage { get; set; }
}
