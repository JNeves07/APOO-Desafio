using AvaliacaoApoo.Domain;
using AvaliacaoApoo.Domain.Entidades;
using AvaliacaoApoo.Domain.Enumeracoes;
using AvaliacaoApoo.Domain.Servicos;

namespace AvaliacaoApoo.Tests;

public class Questao3Tests
{
    private static (Estudante Estudante, Professor Professor, Projeto Projeto) CriarCenario()
    {
        var estudante = new Estudante(1, "Ana Costa", "20261001");
        var professor = new Professor(2, "Dr. Paulo Lima");
        var projeto = estudante.SubmeterProposta(
            10,
            "Sistema de apoio à pesquisa",
            "Projeto de estudo sobre avaliação de relatórios acadêmicos.");
        return (estudante, professor, projeto);
    }

    [Fact]
    public void CriteriosAvaliacao_RelatorioAdequado()
    {
        var (estudante, _, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(
            101, projeto, "Relatório final com resultados e conclusão detalhados.");

        var resultado = new CriteriosAvaliacao().Avaliar(relatorio);

        Assert.True(resultado.Adequado);
        Assert.Empty(resultado.Motivos);
    }

    [Theory]
    [InlineData("Rascunho inicial do relatório")]
    [InlineData("Texto curto")]
    public void CriteriosAvaliacao_RelatorioInadequadoApresentaMotivos(string descricao)
    {
        var (estudante, _, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(101, projeto, descricao);

        var resultado = new CriteriosAvaliacao().Avaliar(relatorio);

        Assert.False(resultado.Adequado);
        Assert.NotEmpty(resultado.Motivos);
    }

    [Fact]
    public void Processar_RelatorioAdequado_AprovaNaPrimeiraTentativaEAtualizaHistorico()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var historico = new HistoricoAcademicoEmMemoria();
        var fluxo = new FluxoAprovacaoRelatorio(historico);
        var relatorio = estudante.ElaborarRelatorio(
            101, projeto, "Relatório final com resultados e conclusão detalhados.");

        var resultado = fluxo.Processar(estudante, professor, relatorio);

        Assert.True(resultado.Aprovado);
        Assert.Equal(StatusRelatorio.Aprovado, relatorio.Status);
        Assert.Equal(1, resultado.Tentativas);
        Assert.Contains("Sistema atualiza status", resultado.Eventos[resultado.Eventos.Count - 1],
            StringComparison.Ordinal);
        Assert.Single(historico.ObterRegistros(estudante));
        Assert.Equal(101, historico.ObterRegistros(estudante)[0].RelatorioId);
    }

    [Fact]
    public void Processar_Rascunho_SolicitaCorrecaoDevolveEReenviaAteAprovacao()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var fluxo = new FluxoAprovacaoRelatorio();
        var relatorio = estudante.ElaborarRelatorio(102, projeto, "Rascunho inicial do relatório");

        var resultado = fluxo.Processar(
            estudante,
            professor,
            relatorio,
            atual => FluxoAprovacaoRelatorio.CorrigirPadrao(atual.Descricao));

        Assert.True(resultado.Aprovado);
        Assert.Equal(StatusRelatorio.Aprovado, relatorio.Status);
        Assert.Equal(2, resultado.Tentativas);
        Assert.Contains(resultado.Eventos, e => e.Contains("solicita correções", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.Eventos, e => e.Contains("devolve", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.Eventos, e => e.Contains("reenvia", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(relatorio.Historico, e => e.Status == StatusRelatorio.AguardandoCorrecao);
    }

    [Fact]
    public void Processar_SempreInadequado_ReprovaAposLimiteDeTentativas()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var historico = new HistoricoAcademicoEmMemoria();
        var fluxo = new FluxoAprovacaoRelatorio(historico);
        var relatorio = estudante.ElaborarRelatorio(103, projeto, "Rascunho inicial do relatório");

        var resultado = fluxo.Processar(estudante, professor, relatorio, _ => "Rascunho sem correção");

        Assert.False(resultado.Aprovado);
        Assert.Equal(StatusRelatorio.Reprovado, relatorio.Status);
        Assert.Equal(FluxoAprovacaoRelatorio.MaximoTentativas, resultado.Tentativas);
        Assert.Equal(3, relatorio.Tentativas);
        Assert.Empty(historico.ObterRegistros(estudante));
    }

    [Fact]
    public void Processar_UsaEstrategiaDeCorrecaoFornecida()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var fluxo = new FluxoAprovacaoRelatorio();
        var relatorio = estudante.ElaborarRelatorio(104, projeto, "Rascunho inicial do relatório");
        var chamadas = 0;

        fluxo.Processar(estudante, professor, relatorio, _ =>
        {
            chamadas++;
            return "Relatório final com resultados e conclusão detalhados.";
        });

        Assert.Equal(1, chamadas);
        Assert.Equal("Relatório final com resultados e conclusão detalhados.", relatorio.Descricao);
        Assert.Equal(StatusRelatorio.Aprovado, relatorio.Status);
    }

    [Fact]
    public void Processar_ComRelatorioJaAprovado_NaoRepeteEnvio()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var fluxo = new FluxoAprovacaoRelatorio();
        var relatorio = estudante.ElaborarRelatorio(
            105, projeto, "Relatório final com resultados e conclusão detalhados.");
        relatorio.Enviar();
        relatorio.MarcarEmAnalise();
        relatorio.Aprovar();

        var resultado = fluxo.Processar(estudante, professor, relatorio);

        Assert.Equal(StatusRelatorio.Aprovado, resultado.StatusFinal);
        Assert.Empty(resultado.Eventos);
    }

    [Fact]
    public void CorrigirPadrao_RemoveMarcadorDeRascunhoECompletaDescricao()
    {
        var corrigido = FluxoAprovacaoRelatorio.CorrigirPadrao("Rascunho");

        Assert.DoesNotContain("rascunho", corrigido, StringComparison.OrdinalIgnoreCase);
        Assert.True(corrigido.Length >= CriteriosAvaliacao.MinimoCaracteresDescricao);
    }

    [Fact]
    public void HistoricoAcademico_NaoRegistraMesmoRelatorioDuasVezes()
    {
        var (estudante, _, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(
            106, projeto, "Relatório final com resultados e conclusão detalhados.");
        var historico = new HistoricoAcademicoEmMemoria();

        historico.Registrar(estudante, relatorio);

        Assert.Throws<DomainException>(() => historico.Registrar(estudante, relatorio));
    }
}
