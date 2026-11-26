using AvaliacaoApoo.Domain;
using AvaliacaoApoo.Domain.CasosDeUso;
using AvaliacaoApoo.Domain.Enumeracoes;

namespace AvaliacaoApoo.Tests;

public class Questao1Tests
{
    [Fact]
    public void CriarPadrao_ContemOsDezCasosDeUsoReconhecidos()
    {
        var diagrama = DiagramaCasosDeUso.CriarPadrao();

        Assert.Equal(10, diagrama.Total);
        Assert.True(diagrama.Contem(TipoAtor.Estudante, "Submeter Proposta de Projeto"));
        Assert.True(diagrama.Contem(TipoAtor.Estudante, "Enviar Relatório"));
        Assert.True(diagrama.Contem(TipoAtor.Estudante, "Consultar Relatórios"));
        Assert.True(diagrama.Contem(TipoAtor.ProfessorOrientador, "Avaliar Relatório"));
        Assert.True(diagrama.Contem(TipoAtor.ProfessorOrientador, "Aprovar Relatório"));
        Assert.True(diagrama.Contem(TipoAtor.ProfessorOrientador, "Solicitar Correções"));
        Assert.True(diagrama.Contem(TipoAtor.ProfessorOrientador, "Registrar Comentários"));
        Assert.True(diagrama.Contem(TipoAtor.Coordenacao, "Acompanhar Projetos"));
        Assert.True(diagrama.Contem(TipoAtor.Coordenacao, "Consultar Status do Relatório"));
        Assert.True(diagrama.Contem(TipoAtor.Coordenacao, "Visualizar Estatísticas"));
    }

    [Fact]
    public void ObterPorAtor_RetornaApenasOsCasosDaqueleAtor()
    {
        var diagrama = DiagramaCasosDeUso.CriarPadrao();

        Assert.Equal(3, diagrama.ObterPorAtor(TipoAtor.Estudante).Count);
        Assert.Equal(4, diagrama.ObterPorAtor(TipoAtor.ProfessorOrientador).Count);
        Assert.Equal(3, diagrama.ObterPorAtor(TipoAtor.Coordenacao).Count);
        Assert.All(diagrama.ObterPorAtor(TipoAtor.Coordenacao),
            caso => Assert.Equal(TipoAtor.Coordenacao, caso.Ator));
    }

    [Fact]
    public void ContarPorAtor_RetornaDistribuicaoCorreta()
    {
        var contagens = DiagramaCasosDeUso.CriarPadrao().ContarPorAtor();

        Assert.Equal(3, contagens[TipoAtor.Estudante]);
        Assert.Equal(4, contagens[TipoAtor.ProfessorOrientador]);
        Assert.Equal(3, contagens[TipoAtor.Coordenacao]);
    }

    [Fact]
    public void Contem_IgnoraCaixaENaoEncontraCasoInexistente()
    {
        var diagrama = DiagramaCasosDeUso.CriarPadrao();

        Assert.True(diagrama.Contem(TipoAtor.Coordenacao, "visualizar estatísticas"));
        Assert.False(diagrama.Contem(TipoAtor.Estudante, "Aprovar Relatório"));
    }

    [Fact]
    public void Construtor_ComCasoDuplicado_LancaDomainException()
    {
        var casos = new[]
        {
            new CasoDeUso(TipoAtor.Estudante, "Enviar Relatório"),
            new CasoDeUso(TipoAtor.Estudante, "enviar relatório")
        };

        Assert.Throws<DomainException>(() => new DiagramaCasosDeUso(casos));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CasoDeUso_ComNomeInvalido_LancaDomainException(string? nome)
    {
        Assert.Throws<DomainException>(() => new CasoDeUso(TipoAtor.Estudante, nome!));
    }
}
