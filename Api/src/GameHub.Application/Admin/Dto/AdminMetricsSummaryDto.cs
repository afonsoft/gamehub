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
