using System.Text.RegularExpressions;
using AvaliacaoApoo.Domain.Entidades;
using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Domain.Servicos;

/// <summary>
/// Resultado da execução do fluxo da questão 3.
/// </summary>
public sealed record ResultadoProcessamento(
    StatusRelatorio StatusFinal,
    int Tentativas,
    IReadOnlyList<string> Eventos)
{
    public bool Aprovado => StatusFinal == StatusRelatorio.Aprovado;
}

/// <summary>
/// Orquestra o fluxo "envio → análise → aprovação ou correção → reenvio"
/// representado no diagrama da questão 3.
/// </summary>
public sealed class FluxoAprovacaoRelatorio
{
    /// <summary>
    /// Aprimoramento documentado: o diagrama original mostra um loop sem limite;
    /// aqui o fluxo é interrompido após três envios para evitar repetição infinita.
    /// </summary>
    public const int MaximoTentativas = 3;

    private readonly CriteriosAvaliacao _criterios = new();
    private readonly IHistoricoAcademico _historico;

    public FluxoAprovacaoRelatorio(IHistoricoAcademico? historico = null)
    {
        _historico = historico ?? new HistoricoAcademicoEmMemoria();
    }

    public ResultadoProcessamento Processar(
        Estudante estudante,
        Professor professor,
        Relatorio relatorio,
        Func<Relatorio, string>? gerarCorrecao = null)
    {
        ArgumentNullException.ThrowIfNull(estudante);
        ArgumentNullException.ThrowIfNull(professor);
        ArgumentNullException.ThrowIfNull(relatorio);

        var eventos = new List<string>();

        if (relatorio.Status == StatusRelatorio.EmElaboracao)
        {
            estudante.EnviarRelatorio(relatorio);
            eventos.Add("Estudante submete relatório no sistema.");
            eventos.Add("Sistema registra e envia ao professor.");
        }

        while (relatorio.Status is not (StatusRelatorio.Aprovado or StatusRelatorio.Reprovado))
        {
            professor.AnalisarRelatorio(relatorio);
            eventos.Add("Professor analisa o relatório.");

            var avaliacao = _criterios.Avaliar(relatorio);

            if (avaliacao.Adequado)
            {
                professor.AprovarRelatorio(relatorio);
                _historico.Registrar(estudante, relatorio);
                eventos.Add("Relatório adequado: professor aprova o relatório.");
                eventos.Add("Sistema atualiza status para \"Aprovado\" e histórico acadêmico.");
                break;
            }

            var motivos = string.Join("; ", avaliacao.Motivos);

            if (relatorio.Tentativas >= MaximoTentativas)
            {
                professor.ReprovarRelatorio(relatorio);
                eventos.Add(
                    $"Limite de {MaximoTentativas} tentativas atingido. Motivos: {motivos}.");
                break;
            }

            professor.SolicitarCorrecoes(relatorio, motivos);
            eventos.Add($"Professor solicita correções: {motivos}.");

            relatorio.DevolverAoEstudante();
            eventos.Add("Sistema devolve relatório ao estudante.");

            var novaDescricao = gerarCorrecao?.Invoke(relatorio) ?? CorrigirPadrao(relatorio.Descricao);
            estudante.RealizarCorrecoes(relatorio, novaDescricao);
            eventos.Add("Estudante realiza correções.");

            estudante.EnviarRelatorio(relatorio);
            eventos.Add("Estudante reenvia relatório.");
        }

        return new ResultadoProcessamento(relatorio.Status, relatorio.Tentativas, eventos);
    }

    /// <summary>
    /// Correção padrão usada quando o chamador não fornece uma estratégia.
    /// </summary>
    public static string CorrigirPadrao(string descricao)
    {
        var texto = descricao.Replace(
            CriteriosAvaliacao.TermoRascunho, string.Empty, StringComparison.OrdinalIgnoreCase);
        texto = Regex.Replace(texto, @"\s+", " ").Trim();

        texto += texto.Length < CriteriosAvaliacao.MinimoCaracteresDescricao
            ? " Relatório revisado com resultados e conclusão."
            : " Resultados e conclusão revisados.";

        return texto;
    }
}
