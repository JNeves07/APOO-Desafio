namespace AvaliacaoApoo.Domain.Entidades;

/// <summary>
/// Base comum a estudantes e professores (herança do diagrama da questão 2).
/// </summary>
public abstract class Pessoa
{
    protected Pessoa(int id, string nome)
    {
        Validacao.Identificador(id, "Id");
        Nome = Validacao.TextoObrigatorio(nome, "Nome", 3, 120);
        Id = id;
    }

    public int Id { get; }

    public string Nome { get; private set; }

    public void AlterarNome(string nome)
        => Nome = Validacao.TextoObrigatorio(nome, "Nome", 3, 120);

    public override string ToString() => $"{Nome} (id={Id})";
}
