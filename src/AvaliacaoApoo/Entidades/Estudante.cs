namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Estudante do sistema: submete propostas, elabora/envia relatórios e aplica correções.
/// </summary>
public sealed class Estudante : Pessoa
{
    public Estudante(int id, string nome, string matricula)
        : base(id, nome)
    {
        Matricula = Validacao.TextoObrigatorio(matricula, "Matrícula", 3, 30);
    }

    public string Matricula { get; }

    /// <summary>
    /// Caso de uso "Submeter Proposta de Projeto".
    /// </summary>
    public Projeto SubmeterProposta(int id, string titulo, string descricao)
        => new(id, titulo, descricao, this);

    /// <summary>
    /// O estudante elabora um relatório vinculado a um projeto (composição).
    /// </summary>
    public Relatorio ElaborarRelatorio(int id, Projeto projeto, string descricao)
    {
        ArgumentNullException.ThrowIfNull(projeto);

        var relatorio = new Relatorio(id, descricao, projeto);
        projeto.AdicionarRelatorio(relatorio);
        return relatorio;
    }

    /// <summary>
    /// Caso de uso "Enviar Relatório" (também usado no reenvio após correções).
    /// </summary>
    public void EnviarRelatorio(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.Enviar();
    }

    /// <summary>
    /// Etapa da questão 3: "Estudante realiza correções".
    /// </summary>
    public void RealizarCorrecoes(Relatorio relatorio, string novaDescricao)
    {
        ArgumentNullException.ThrowIfNull(relatorio);
        relatorio.AplicarCorrecao(novaDescricao);
    }

    public override string ToString() => $"{base.ToString()} matrícula={Matricula}";
}
