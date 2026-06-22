using MediaEscolar.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var alunos = new List<Aluno>();
var proximoId = 1;

app.MapPost("/alunos", (CadastrarAlunoRequest request) =>
{
    var aluno = new Aluno { Id = proximoId++, Nome = request.Nome};
    alunos.Add(aluno);
    return Results.Created($"/alunos/{aluno.Id}", aluno);
});

app.MapPost("/alunos/{id:int}/notas", (int id, RegistrarNotasRequest request) =>
{
    var aluno = alunos.FirstOrDefault(a => a.Id == id);
    if (aluno is null)
    {
        return Results.NotFound();
    }

    try
    {
        Calculadora.ValidarNota(request.Nota1);
        Calculadora.ValidarNota(request.Nota2);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }

    aluno.Nota1 = request.Nota1;
    aluno.Nota2 = request.Nota2;
    return Results.Ok(aluno);
});

app.MapGet("/alunos/{id:int}", (int id) =>
{
    var aluno = alunos.FirstOrDefault(a => a.Id == id);
    if (aluno is null)
    {
        return Results.NotFound();
    }

    if (aluno.Nota1 is null || aluno.Nota2 is null)
    {
        return Results.Ok(new AlunoResponse(aluno.Id, aluno.Nome, aluno.Nota1, aluno.Nota2, null, null));
    }

    var media = Calculadora.CalcularMedia(aluno.Nota1.Value, aluno.Nota2.Value);
    var situacao = Calculadora.CalcularSituacao(media);
    return Results.Ok(new AlunoResponse(aluno.Id, aluno.Nome, aluno.Nota1, aluno.Nota2, media, situacao));
});

app.MapGet("/alunos", () => alunos);

app.Run();

public record CadastrarAlunoRequest(string Nome);
public record RegistrarNotasRequest(double Nota1, double Nota2);
public record AlunoResponse(int Id, string Nome, double? Nota1, double? Nota2, double? Media, string? Situacao);

public partial class Program;
