using AvaliacaoApoo.Domain;
using AvaliacaoApoo.Domain.Entidades;
using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Tests;

public class Questao2Tests
{
    private static (Estudante Estudante, Professor Professor, Projeto Projeto) CriarCenario()
    {
        var estudante = new Estudante(1, "Ana Costa", "20261001");
        var professor = new Professor(2, "Dr. Paulo Lima");
        var projeto = estudante.SubmeterProposta(
            10,
            "Sistema de apoio à pesquisa",
            "Projeto de estudo sobre avaliação de relatórios acadêmicos.");
        projeto.DefinirOrientador(professor);
        return (estudante, professor, projeto);
    }

    [Theory]
    [InlineData(0, "Ana Costa", "20261001")]
    [InlineData(1, "A", "20261001")]
    [InlineData(1, "Ana Costa", "")]
    public void Estudante_ComDadosInvalidos_LancaDomainException(
        int id, string nome, string matricula)
    {
        Assert.Throws<DomainException>(() => new Estudante(id, nome, matricula));
    }

    [Fact]
    public void Professor_ComNomeInvalido_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => new Professor(2, " "));
    }

    [Fact]
    public void Pessoa_AlterarNome_AplicaValidacao()
    {
        var professor = new Professor(2, "Dr. Paulo Lima");

        professor.AlterarNome("  Dra. Maria Lima  ");

        Assert.Equal("Dra. Maria Lima", professor.Nome);
        Assert.Throws<DomainException>(() => professor.AlterarNome("x"));
    }

    [Fact]
    public void SubmeterProposta_AssociaProjetoAoEstudanteResponsavel()
    {
        var (estudante, professor, projeto) = CriarCenario();

        Assert.Same(estudante, projeto.Responsavel);
        Assert.Same(professor, projeto.Orientador);
        Assert.Equal("Sistema de apoio à pesquisa", projeto.Titulo);
    }

    [Theory]
    [InlineData(10, "A", "Descrição válida com mais de dez caracteres")]
    [InlineData(10, "Título válido", "curta")]
    public void Projeto_ComTextoInvalido_LancaDomainException(
        int id, string titulo, string descricao)
    {
        var estudante = new Estudante(1, "Ana Costa", "20261001");

        Assert.Throws<DomainException>(() => new Projeto(id, titulo, descricao, estudante));
    }

    [Fact]
    public void ElaborarRelatorio_CompõeRelatorioNoProjeto()
    {
        var (estudante, _, projeto) = CriarCenario();

        var relatorio = estudante.ElaborarRelatorio(
            100, projeto, "Relatório final com resultados e conclusão detalhados.");

        Assert.Same(projeto, relatorio.Projeto);
        Assert.Single(projeto.Relatorios);
        Assert.Same(relatorio, projeto.Relatorios[0]);
        Assert.Equal(StatusRelatorio.EmElaboracao, relatorio.Status);
        Assert.Single(relatorio.Historico);
    }

    [Fact]
    public void AdicionarRelatorio_DeOutroProjeto_LancaDomainException()
    {
        var (estudante, _, projeto) = CriarCenario();
        var outroProjeto = estudante.SubmeterProposta(
            11, "Outro projeto", "Outra descrição válida para o projeto de teste.");
        var relatorio = estudante.ElaborarRelatorio(
            100, projeto, "Relatório final com resultados e conclusão detalhados.");

        Assert.Throws<DomainException>(() => outroProjeto.AdicionarRelatorio(relatorio));
    }

    [Fact]
    public void AdicionarRelatorio_ComIdRepetido_LancaDomainException()
    {
        var (estudante, _, projeto) = CriarCenario();
        var descricao = "Relatório final com resultados e conclusão detalhados.";
        var primeiro = new Relatorio(100, descricao, projeto);
        var segundo = new Relatorio(100, descricao, projeto);

        projeto.AdicionarRelatorio(primeiro);

        Assert.Throws<DomainException>(() => projeto.AdicionarRelatorio(segundo));
    }

    [Fact]
    public void TransicoesDeStatus_RespeitamRegrasDoFluxo()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(
            100, projeto, "Relatório final com resultados e conclusão detalhados.");

        Assert.Throws<DomainException>(() => professor.AprovarRelatorio(relatorio));

        relatorio.Enviar();
        Assert.Equal(StatusRelatorio.Enviado, relatorio.Status);
        Assert.NotNull(relatorio.EnviadoEm);

        professor.AnalisarRelatorio(relatorio);
        Assert.Equal(StatusRelatorio.EmAnalise, relatorio.Status);

        professor.ComentarRelatorio(relatorio, "Descrição consistente.");
        Assert.Single(relatorio.Comentarios);
        Assert.Equal("Dr. Paulo Lima", relatorio.Comentarios[0].Autor);

        relatorio.AtualizarStatus(StatusRelatorio.Aprovado);
        Assert.Equal(StatusRelatorio.Aprovado, relatorio.Status);
        Assert.Throws<DomainException>(() => relatorio.Enviar());
    }

    [Fact]
    public void DevolverAEstudante_SomenteAposSolicitarCorrecoes()
    {
        var (estudante, professor, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(
            100, projeto, "Rascunho inicial do relatório");
        relatorio.Enviar();
        professor.AnalisarRelatorio(relatorio);

        Assert.Throws<DomainException>(() => relatorio.DevolverAoEstudante());
        Assert.Throws<DomainException>(() =>
            relatorio.AplicarCorrecao("Relatório corrigido com resultados e conclusão."));

        professor.SolicitarCorrecoes(relatorio, "Descrição insuficiente.");
        Assert.Equal(StatusRelatorio.AguardandoCorrecao, relatorio.Status);

        relatorio.DevolverAoEstudante();
        relatorio.AplicarCorrecao("Relatório corrigido com resultados e conclusão.");
        Assert.Contains("resultados", relatorio.Descricao, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Coordenacao_VisualizarEstatisticas_CalculaIndicadores()
    {
        var (estudante, _, projeto) = CriarCenario();
        var aprovado = estudante.ElaborarRelatorio(
            101, projeto, "Relatório final com resultados e conclusão detalhados.");
        var emAnalise = estudante.ElaborarRelatorio(
            102, projeto, "Segundo relatório com resultados e conclusão detalhados.");
        aprovado.Enviar();
        aprovado.MarcarEmAnalise();
        aprovado.Aprovar();
        emAnalise.Enviar();
        emAnalise.MarcarEmAnalise();

        var coordenacao = new Coordenacao();
        var estatisticas = coordenacao.VisualizarEstatisticas(new[] { projeto });

        Assert.Single(coordenacao.AcompanharProjetos(new[] { projeto }));
        Assert.Equal(1, estatisticas.TotalProjetos);
        Assert.Equal(2, estatisticas.TotalRelatorios);
        Assert.Equal(1, estatisticas.Aprovados);
        Assert.Equal(1, estatisticas.EmAnalise);
        Assert.Equal(50, estatisticas.TaxaAprovacao);
    }

    [Fact]
    public void HistoricoDoRelatorio_RegistraEventosEmOrdem()
    {
        var (estudante, _, projeto) = CriarCenario();
        var relatorio = estudante.ElaborarRelatorio(
            100, projeto, "Relatório final com resultados e conclusão detalhados.");

        relatorio.Enviar();
        relatorio.MarcarEmAnalise();
        relatorio.Aprovar();

        Assert.Equal(6, relatorio.Historico.Count);
        Assert.All(relatorio.Historico,
            evento => Assert.True(evento.OcorridoEm <= DateTimeOffset.UtcNow));
    }
}
