using AvaliacaoApoo.Domain.Entidades;

namespace AvaliacaoApoo.Domain.Servicos;

/// <summary>
/// Resultado da decisão "Relatório está adequado?" da questão 3.
/// </summary>
public sealed record ResultadoAvaliacao(bool Adequado, IReadOnlyList<string> Motivos)
{
    public static ResultadoAvaliacao Aprovado()
        => new(true, Array.Empty<string>());

    public static ResultadoAvaliacao Reprovado(IEnumerable<string> motivos)
        => new(false, motivos.ToList().AsReadOnly());
}

/// <summary>
/// Regras de negócio que decidem se o relatório está adequado.
/// </summary>
public sealed class CriteriosAvaliacao
{
    public const int MinimoCaracteresDescricao = 20;
    public const string TermoRascunho = "rascunho";

    public ResultadoAvaliacao Avaliar(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);

        var motivos = new List<string>();

        if (relatorio.Descricao.Trim().Length < MinimoCaracteresDescricao)
        {
            motivos.Add($"descrição com menos de {MinimoCaracteresDescricao} caracteres");
        }

        if (relatorio.Descricao.Contains(TermoRascunho, StringComparison.OrdinalIgnoreCase))
        {
            motivos.Add("descrição ainda marca o relatório como rascunho");
        }

        return motivos.Count == 0
            ? ResultadoAvaliacao.Aprovado()
            : ResultadoAvaliacao.Reprovado(motivos);
    }
}
