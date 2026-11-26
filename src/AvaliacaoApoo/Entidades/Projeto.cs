namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Projeto de pesquisa submetido por um estudante. Agrega seus relatórios.
/// </summary>
public sealed class Projeto
{
    private readonly List<Relatorio> _relatorios = new();

    public Projeto(int id, string titulo, string descricao, Estudante responsavel)
    {
        Validacao.Identificador(id, "Id");
        Titulo = Validacao.TextoObrigatorio(titulo, "Título", 3, 200);
        Descricao = Validacao.TextoObrigatorio(descricao, "Descrição", 10, 2000);
        Responsavel = responsavel ?? throw new ArgumentNullException(nameof(responsavel));
        Id = id;
    }

    public int Id { get; }

    public string Titulo { get; private set; }

    public string Descricao { get; private set; }

    /// <summary>Agregação: o projeto conhece seu responsável.</summary>
    public Estudante Responsavel { get; }

    /// <summary>Agregação: o projeto conhece seu orientador, quando definido.</summary>
    public Professor? Orientador { get; private set; }

    /// <summary>Composição: relatórios pertencem ao projeto.</summary>
    public IReadOnlyList<Relatorio> Relatorios => _relatorios.AsReadOnly();

    public void DefinirOrientador(Professor orientador)
        => Orientador = orientador ?? throw new ArgumentNullException(nameof(orientador));

    public void AlterarTitulo(string titulo)
        => Titulo = Validacao.TextoObrigatorio(titulo, "Título", 3, 200);

    public void AlterarDescricao(string descricao)
        => Descricao = Validacao.TextoObrigatorio(descricao, "Descrição", 10, 2000);

    public void AdicionarRelatorio(Relatorio relatorio)
    {
        ArgumentNullException.ThrowIfNull(relatorio);

        if (!ReferenceEquals(relatorio.Projeto, this))
        {
            throw new DomainException("O relatório pertence a outro projeto.");
        }

        if (_relatorios.Any(r => r.Id == relatorio.Id))
        {
            throw new DomainException("Já existe um relatório com este identificador no projeto.");
        }

        _relatorios.Add(relatorio);
    }

    public override string ToString() => $"{Titulo} (id={Id})";
}
