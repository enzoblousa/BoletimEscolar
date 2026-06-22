using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MediaEscolar.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_CadastrarRegistrarNotasEConsultar_DeveRetornarMediaESituacaoCorretas()
    {
        var cadastroResponse = await _client.PostAsJsonAsync("/alunos", new { nome = "Maria" }, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, cadastroResponse.StatusCode);

        var alunoCriado = await cadastroResponse.Content.ReadFromJsonAsync<AlunoCriadoDto>(JsonOptions);
        Assert.NotNull(alunoCriado);

        var notasResponse = await _client.PostAsJsonAsync(
            $"/alunos/{alunoCriado!.Id}/notas", new { nota1 = 8.0, nota2 = 6.0 }, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, notasResponse.StatusCode);

        var consultaResponse = await _client.GetAsync($"/alunos/{alunoCriado.Id}");
        Assert.Equal(HttpStatusCode.OK, consultaResponse.StatusCode);

        var alunoConsultado = await consultaResponse.Content.ReadFromJsonAsync<AlunoConsultadoDto>(JsonOptions);
        Assert.NotNull(alunoConsultado);
        Assert.Equal(7.0, alunoConsultado!.Media);
        Assert.Equal("Aprovado", alunoConsultado.Situacao);
    }

    private record AlunoCriadoDto(int Id, string Nome, double? Nota1, double? Nota2);
    private record AlunoConsultadoDto(int Id, string Nome, double? Nota1, double? Nota2, double? Media, string? Situacao);
}
