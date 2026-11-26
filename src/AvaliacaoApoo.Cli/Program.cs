using AvaliacaoApoo.Domain.CasosDeUso;
using AvaliacaoApoo.Domain.Entidades;
using AvaliacaoApoo.Domain.Enumeracoes;
using AvaliacaoApoo.Domain.Servicos;

Console.WriteLine("============================================================");
Console.WriteLine(" Avaliação Prática de Análise e Projeto Orientado a Objetos");
Console.WriteLine(" Solução em C# / .NET 10 — modelagem e execução das 3 questões");
Console.WriteLine("============================================================");

ApresentarQuestao1();
ApresentarQuestao2();
ApresentarQuestao3();

static void ApresentarQuestao1()
{
    Console.WriteLine();
    Console.WriteLine("=== Questão 1 — Diagrama de Casos de Uso ===");

    var diagrama = DiagramaCasosDeUso.CriarPadrao();

    foreach (var ator in Enum.GetValues<TipoAtor>())
    {
        Console.WriteLine();
        Console.WriteLine($"Ator: {NomeAmigavel(ator)}");
        foreach (var caso in diagrama.ObterPorAtor(ator))
        {
            Console.WriteLine($"  • {caso.Nome}");
            Console.WriteLine($"      {caso.Descricao}");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Total de casos de uso: {diagrama.Total}");
}

static void ApresentarQuestao2()
{
    Console.WriteLine();
    Console.WriteLine("=== Questão 2 — Diagrama de Classes ===");

    Console.WriteLine();
    Console.WriteLine($"{"Classe",-16} | {"Atributos",-52} | Operações");
    Console.WriteLine(new string('-', 120));
    Console.WriteLine($"{"Professor",-16} | {"id: int, nome: string",-52} | analisarRelatorio, comentarRelatorio, aprovarRelatorio");
    Console.WriteLine($"{"Estudante",-16} | {"id: int, nome: string, matricula: string",-52} | submeterProposta, enviarRelatorio");
    Console.WriteLine($"{"Projeto",-16} | {"id: int, titulo: string, descricao: string",-52} | agrega Relatorio (0..*)");
    Console.WriteLine($"{"Relatorio",-16} | {"id, descricao, dataEnvio, status, tentativas",-52} | enviar, atualizarStatus, aplicarCorrecao");
    Console.WriteLine($"{"Coordenacao",-16} | {"(sem atributos no enunciado)",-52} | acompanharProjetos, visualizarEstatisticas");

    var estudante = new Estudante(1, "Ana Costa", "20261001");
    var professor = new Professor(2, "Dr. Paulo Lima");
    var projeto = estudante.SubmeterProposta(
        10,
        "Sistema de apoio à pesquisa",
        "Projeto de estudo sobre processos de avaliação de relatórios acadêmicos.");
    projeto.DefinirOrientador(professor);
    var relatorio = estudante.ElaborarRelatorio(
        100,
        projeto,
        "Relatório final com resultados e conclusão detalhados.");

    Console.WriteLine();
    Console.WriteLine("Instâncias criadas (agregação/composição):");
    Console.WriteLine($"  {estudante}");
    Console.WriteLine($"  {professor}");
    Console.WriteLine($"  {projeto} — orientador: {projeto.Orientador}");
    Console.WriteLine($"  {relatorio}");
    Console.WriteLine($"  Relatórios no projeto: {projeto.Relatorios.Count}");

    var coordenacao = new Coordenacao();
    var estatisticas = coordenacao.VisualizarEstatisticas(new[] { projeto });
    Console.WriteLine($"  Estatísticas: {estatisticas.TotalRelatorios} relatório(s), " +
                      $"{estatisticas.TaxaAprovacao:F1}% aprovados");
}

static void ApresentarQuestao3()
{
    Console.WriteLine();
    Console.WriteLine("=== Questão 3 — Fluxo de envio, análise e aprovação ===");

    var historico = new HistoricoAcademicoEmMemoria();
    var fluxo = new FluxoAprovacaoRelatorio(historico);

    var estudante = new Estudante(1, "Ana Costa", "20261001");
    var professor = new Professor(2, "Dr. Paulo Lima");
    var projeto = estudante.SubmeterProposta(
        10,
        "Sistema de apoio à pesquisa",
        "Projeto de estudo sobre processos de avaliação de relatórios acadêmicos.");

    Console.WriteLine();
    Console.WriteLine("Cenário A — relatório adequado (aprovado na primeira análise):");
    var relatorioA = estudante.ElaborarRelatorio(
        101,
        projeto,
        "Relatório final com resultados e conclusão detalhados.");
    ImprimirResultado(fluxo.Processar(estudante, professor, relatorioA));

    Console.WriteLine();
    Console.WriteLine("Cenário B — rascunho devolvido, corrigido e reenviado:");
    var relatorioB = estudante.ElaborarRelatorio(102, projeto, "Rascunho inicial do relatório");
    ImprimirResultado(fluxo.Processar(
        estudante,
        professor,
        relatorioB,
        relatorio => FluxoAprovacaoRelatorio.CorrigirPadrao(relatorio.Descricao)));

    Console.WriteLine();
    Console.WriteLine($"Cenário C — nunca corrigido (limite de {FluxoAprovacaoRelatorio.MaximoTentativas} tentativas):");
    var relatorioC = estudante.ElaborarRelatorio(103, projeto, "Rascunho inicial do relatório");
    ImprimirResultado(fluxo.Processar(
        estudante,
        professor,
        relatorioC,
        _ => "Rascunho sem correção"));

    Console.WriteLine();
    Console.WriteLine($"Registros no histórico acadêmico: {historico.ObterRegistros(estudante).Count}");
    foreach (var registro in historico.ObterRegistros(estudante))
    {
        Console.WriteLine($"  Relatório #{registro.RelatorioId} aprovado em {registro.AprovadoEm:u}");
    }
}

static void ImprimirResultado(ResultadoProcessamento resultado)
{
    var numero = 1;
    foreach (var evento in resultado.Eventos)
    {
        Console.WriteLine($"  {numero++,2}. {evento}");
    }

    Console.WriteLine($"  Resultado: {resultado.StatusFinal} | " +
                      $"tentativas: {resultado.Tentativas} | " +
                      $"eventos registrados: {resultado.Eventos.Count}");
}

static string NomeAmigavel(TipoAtor ator) => ator switch
{
    TipoAtor.Estudante => "Estudante",
    TipoAtor.ProfessorOrientador => "Professor Orientador",
    TipoAtor.Coordenacao => "Coordenação",
    _ => ator.ToString()
};
