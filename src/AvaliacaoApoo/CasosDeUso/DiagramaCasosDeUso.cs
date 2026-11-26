using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.CasosDeUso;

/// <summary>
/// Agregado que representa o diagrama de casos de uso da questão 1.
/// </summary>
public sealed class DiagramaCasosDeUso
{
    private readonly List<CasoDeUso> _casos = new();

    public DiagramaCasosDeUso(IEnumerable<CasoDeUso> casos)
    {
        ArgumentNullException.ThrowIfNull(casos);

        foreach (var caso in casos)
        {
            ArgumentNullException.ThrowIfNull(caso);

            if (_casos.Any(c => c.Ator == caso.Ator &&
                                string.Equals(c.Nome, caso.Nome, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException(
                    $"Caso de uso duplicado para o ator {caso.Ator}: '{caso.Nome}'.");
            }

            _casos.Add(caso);
        }
    }

    public IReadOnlyList<CasoDeUso> Todos => _casos.AsReadOnly();

    public int Total => _casos.Count;

    /// <summary>
    /// Casos de uso reconstruídos a partir da imagem da questão 1.
    /// </summary>
    public static DiagramaCasosDeUso CriarPadrao() => new(new[]
    {
        new CasoDeUso(TipoAtor.Estudante, "Submeter Proposta de Projeto",
            "O estudante envia a proposta de projeto para avaliação."),
        new CasoDeUso(TipoAtor.Estudante, "Enviar Relatório",
            "O estudante submete o relatório elaborado no sistema."),
        new CasoDeUso(TipoAtor.Estudante, "Consultar Relatórios",
            "O estudante consulta seus relatórios e seus status."),

        new CasoDeUso(TipoAtor.ProfessorOrientador, "Avaliar Relatório",
            "O professor orientador analisa o conteúdo do relatório."),
        new CasoDeUso(TipoAtor.ProfessorOrientador, "Aprovar Relatório",
            "O professor orientador aprova um relatório adequado."),
        new CasoDeUso(TipoAtor.ProfessorOrientador, "Solicitar Correções",
            "O professor orientador devolve o relatório para ajustes."),
        new CasoDeUso(TipoAtor.ProfessorOrientador, "Registrar Comentários",
            "O professor orientador registra comentários sobre o relatório."),

        new CasoDeUso(TipoAtor.Coordenacao, "Acompanhar Projetos",
            "A coordenação acompanha o andamento dos projetos."),
        new CasoDeUso(TipoAtor.Coordenacao, "Consultar Status do Relatório",
            "A coordenação consulta o status dos relatórios."),
        new CasoDeUso(TipoAtor.Coordenacao, "Visualizar Estatísticas",
            "A coordenação visualiza indicadores dos projetos e relatórios.")
    });

    public IReadOnlyList<CasoDeUso> ObterPorAtor(TipoAtor ator)
        => _casos.Where(c => c.Ator == ator).ToList().AsReadOnly();

    public bool Contem(TipoAtor ator, string nome)
        => _casos.Any(c => c.Ator == ator &&
                           string.Equals(c.Nome, nome, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyDictionary<TipoAtor, int> ContarPorAtor()
        => _casos
            .GroupBy(c => c.Ator)
            .ToDictionary(g => g.Key, g => g.Count());
}
