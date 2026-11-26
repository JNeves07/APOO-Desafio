using AvaliacaoApoo.Domain.Entidades;

namespace AvaliacaoApoo.Domain.Servicos;

/// <summary>
/// Registro de aprovação no histórico acadêmico do estudante.
/// </summary>
public sealed record RegistroHistorico(
    int EstudanteId,
    string Matricula,
    int RelatorioId,
    DateTimeOffset AprovadoEm);

/// <summary>
/// Contrato para atualização do histórico acadêmico ("Sistema atualiza histórico acadêmico").
/// </summary>
public interface IHistoricoAcademico
{
    void Registrar(Estudante estudante, Relatorio relatorio);

    IReadOnlyList<RegistroHistorico> ObterRegistros(Estudante estudante);
}

/// <summary>
/// Implementação em memória do histórico acadêmico.
/// </summary>
public sealed class HistoricoAcademicoEmMemoria : IHistoricoAcademico
{
    private readonly List<RegistroHistorico> _registros = new();

    public void Registrar(Estudante estudante, Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(estudante);
        ArgumentNullException.ThrowIfNull(relatorio);

        if (_registros.Any(r => r.RelatorioId == relatorio.Id))
        {
            throw new DomainException("Este relatório já foi registrado no histórico acadêmico.");
        }

        _registros.Add(new RegistroHistorico(
            estudante.Id,
            estudante.Matricula,
            relatorio.Id,
            DateTimeOffset.UtcNow));
    }

    public IReadOnlyList<RegistroHistorico> ObterRegistros(Estudante estudante)
    {
        ArgumentNullException.ThrowIfNull(estudante);
        return _registros.Where(r => r.EstudanteId == estudante.Id).ToList().AsReadOnly();
    }
}
