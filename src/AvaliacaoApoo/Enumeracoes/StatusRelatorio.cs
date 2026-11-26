namespace AvaliacaoApoo.Domain.Enumeracoes;

/// <summary>
/// Estados possíveis de um relatório no fluxo da questão 3.
/// </summary>
public enum StatusRelatorio
{
    EmElaboracao,
    Enviado,
    EmAnalise,
    AguardandoCorrecao,
    Aprovado,
    Reprovado
}
