using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Indicadores calculados pela coordenação (casos de uso da questão 1).
/// </summary>
public sealed record EstatisticasProjetos(
    int TotalProjetos,
    int TotalRelatorios,
    int Aprovados,
    int EmAnalise,
    int AguardandoCorrecao,
    int Reprovados)
{
    public double TaxaAprovacao
        => TotalRelatorios == 0 ? 0 : Aprovados * 100.0 / TotalRelatorios;
}

/// <summary>
/// Contrato de monitoramento implementado pela coordenação (questão 2).
/// </summary>
public interface IMonitoramentoProjetos
{
    IReadOnlyList<Projeto> AcompanharProjetos(IEnumerable<Projeto> projetos);

    EstatisticasProjetos VisualizarEstatisticas(IEnumerable<Projeto> projetos);
}

/// <summary>
/// Coordenação: acompanha projetos e visualiza estatísticas.
/// </summary>
public sealed class Coordenacao : IMonitoramentoProjetos
{
    public IReadOnlyList<Projeto> AcompanharProjetos(IEnumerable<Projeto> projetos)
    {
        ArgumentNullException.ThrowIfNull(projetos);
        return projetos.OrderBy(p => p.Id).ToList().AsReadOnly();
    }

    public EstatisticasProjetos VisualizarEstatisticas(IEnumerable<Projeto> projetos)
    {
        var lista = AcompanharProjetos(projetos);
        var relatorios = lista.SelectMany(p => p.Relatorios).ToList();

        return new EstatisticasProjetos(
            TotalProjetos: lista.Count,
            TotalRelatorios: relatorios.Count,
            Aprovados: relatorios.Count(r => r.Status == StatusRelatorio.Aprovado),
            EmAnalise: relatorios.Count(r => r.Status == StatusRelatorio.EmAnalise),
            AguardandoCorrecao: relatorios.Count(r => r.Status == StatusRelatorio.AguardandoCorrecao),
            Reprovados: relatorios.Count(r => r.Status == StatusRelatorio.Reprovado));
    }
}
