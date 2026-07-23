using System;
using System.Collections.Generic;

namespace GameHub.Developer.Dto;

/// <summary>
/// Resumo do painel do desenvolvedor com jogos, métricas e ações pendentes.
/// </summary>
public class DeveloperDashboardDto
{
    /// <summary>Total de jogos do desenvolvedor.</summary>
    public int TotalGames { get; set; }

    /// <summary>Jogos publicados.</summary>
    public int PublishedGames { get; set; }

    /// <summary>Jogos aguardando revisão.</summary>
    public int PendingReviewGames { get; set; }

    /// <summary>Jogos em rascunho.</summary>
    public int DraftGames { get; set; }

    /// <summary>Jogos rejeitados.</summary>
    public int RejectedGames { get; set; }

    /// <summary>Total acumulado de plays nos jogos do desenvolvedor.</summary>
    public long TotalPlays { get; set; }

    /// <summary>Versões (builds) recentes.</summary>
    public List<DeveloperGameVersionDto> RecentVersions { get; set; } = new();

    /// <summary>Jogos que precisam de ação (rascunho/rejeitado/em revisão).</summary>
    public List<DeveloperDashboardActionDto> PendingActions { get; set; } = new();

    /// <summary>Plays nos últimos 7 dias para gráfico.</summary>
    public List<DeveloperDashboardDailyPlaysDto> PlaysOverTime { get; set; } = new();
}

/// <summary>
/// Item de plays por dia para o gráfico do desenvolvedor.
/// </summary>
public class DeveloperDashboardDailyPlaysDto
{
    /// <summary>Date.</summary>
    public DateTime Date { get; set; }

    /// <summary>Total plays.</summary>
    public long Plays { get; set; }
}
