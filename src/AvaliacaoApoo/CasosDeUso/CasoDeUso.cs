using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.CasosDeUso;

/// <summary>
/// Caso de uso identificado no diagrama da questão 1.
/// </summary>
public sealed class CasoDeUso
{
    public CasoDeUso(TipoAtor ator, string nome, string descricao = "")
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do caso de uso é obrigatório.");
        }

        Ator = ator;
        Nome = nome.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
    }

    public TipoAtor Ator { get; }

    public string Nome { get; }

    public string Descricao { get; }

    public override string ToString() => Nome;
}
