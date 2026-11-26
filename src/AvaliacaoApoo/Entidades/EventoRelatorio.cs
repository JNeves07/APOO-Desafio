using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Evento do histórico de um relatório (composição).
/// </summary>
public sealed record EventoRelatorio(
    DateTimeOffset OcorridoEm,
    StatusRelatorio Status,
    string Descricao);
