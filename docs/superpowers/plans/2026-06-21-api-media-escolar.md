# API de Média Escolar — CI/CD com Docker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a minimal .NET 8 "API de Média Escolar" with in-memory storage, automated tests (unit + integration + regression), a Dockerfile, and a GitHub Actions CI/CD pipeline that builds, lints, tests, and publishes evidence — including a documented failure/fix cycle — fulfilling the activity in `DocumentoProfessor/red-enzo.pdf`.

**Architecture:** ASP.NET Core Minimal API (`src/MediaEscolar.Api`) with business logic isolated in a static `Calculadora` class for easy unit testing, an `Aluno` in-memory list for storage, and an xUnit test project (`tests/MediaEscolar.Tests`) covering unit, regression, and integration (via `WebApplicationFactory`) tests. Packaged with a multi-stage Dockerfile and validated by a GitHub Actions workflow.

**Tech Stack:** .NET 8 SDK, ASP.NET Core Minimal API, xUnit, coverlet.collector, Microsoft.AspNetCore.Mvc.Testing, Docker, GitHub Actions.

**Reference spec:** `docs/superpowers/specs/2026-06-21-api-media-escolar-design.md`

---

## Task 1: Scaffold the .NET solution and projects

**Files:**
- Create: `MediaEscolar.sln`
- Create: `src/MediaEscolar.Api/MediaEscolar.Api.csproj`
- Create: `src/MediaEscolar.Api/Program.cs` (template default, will be replaced in Task 4)
- Create: `tests/MediaEscolar.Tests/MediaEscolar.Tests.csproj`
- Create: `tests/MediaEscolar.Tests/UnitTest1.cs` (template default, will be deleted)

- [ ] **Step 1: Verify the .NET 8 SDK is installed**

Run: `dotnet --version`
Expected: a version string starting with `8.` (e.g. `8.0.404`). If it's not installed or not 8.x, stop and tell the user — do not proceed with a different major version.

- [ ] **Step 2: Create the solution file**

Run: `dotnet new sln -n MediaEscolar`
Expected: `The template "Solution File" was created successfully.`

- [ ] **Step 3: Create the API project**

Run: `dotnet new web -n MediaEscolar.Api -o src/MediaEscolar.Api`
Expected: `The template "ASP.NET Core Empty" was created successfully.`

- [ ] **Step 4: Create the test project**

Run: `dotnet new xunit -n MediaEscolar.Tests -o tests/MediaEscolar.Tests`
Expected: `The template "xUnit Test Project" was created successfully.`

- [ ] **Step 5: Delete the template's placeholder test file**

Delete `tests/MediaEscolar.Tests/UnitTest1.cs` — it only contains an empty `Test1` method and is not part of our test suite.

- [ ] **Step 6: Add both projects to the solution**

Run: `dotnet sln MediaEscolar.sln add src/MediaEscolar.Api/MediaEscolar.Api.csproj tests/MediaEscolar.Tests/MediaEscolar.Tests.csproj`
Expected: two lines, each `Project ... added to the solution.`

- [ ] **Step 7: Add a project reference from tests to the API**

Run: `dotnet add tests/MediaEscolar.Tests/MediaEscolar.Tests.csproj reference src/MediaEscolar.Api/MediaEscolar.Api.csproj`
Expected: `Reference ... added to the project.`

- [ ] **Step 8: Add the integration-testing package to the test project**

Run: `dotnet add tests/MediaEscolar.Tests/MediaEscolar.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing`
Expected: a line confirming `Microsoft.AspNetCore.Mvc.Testing` was added to `MediaEscolar.Tests.csproj`.

- [ ] **Step 9: Allow the test project to see the API's internal `Program` class**

Top-level-statement programs generate an `internal` `Program` class. `WebApplicationFactory<Program>` in the test project needs access to it.

Edit `src/MediaEscolar.Api/MediaEscolar.Api.csproj` and add this `ItemGroup` before the closing `</Project>` tag:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="MediaEscolar.Tests" />
  </ItemGroup>
```

- [ ] **Step 10: Verify everything builds**

Run: `dotnet build MediaEscolar.sln`
Expected: `Build succeeded.` with `0 Error(s)`.

- [ ] **Step 11: Commit**

```bash
git add MediaEscolar.sln src/MediaEscolar.Api tests/MediaEscolar.Tests
git commit -m "Scaffold MediaEscolar solution with API and test projects"
```

---

## Task 2: Add .gitignore and .dockerignore

**Files:**
- Create: `.gitignore`
- Create: `.dockerignore`

- [ ] **Step 1: Create `.gitignore`**

```gitignore
bin/
obj/
*.user
.vs/
TestResults/
```

- [ ] **Step 2: Create `.dockerignore`**

```dockerignore
**/bin/
**/obj/
.git
.github
docs/
*.md
TestResults/
```

- [ ] **Step 3: Remove already-tracked build artifacts, if any**

Run: `git status`
Expected: no `bin/` or `obj/` directories listed as untracked-but-ignored issues. If any `bin/`/`obj/` paths show up as tracked, run `git rm -r --cached <path>` for each before committing.

- [ ] **Step 4: Commit**

```bash
git add .gitignore .dockerignore
git commit -m "Add .gitignore and .dockerignore for .NET build artifacts"
```

---

## Task 3: TDD — Calculadora (regra de negócio de média e situação)

**Files:**
- Create: `tests/MediaEscolar.Tests/CalculadoraTests.cs`
- Create: `src/MediaEscolar.Api/Models/Calculadora.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/MediaEscolar.Tests/CalculadoraTests.cs`:

```csharp
using MediaEscolar.Api.Models;
using Xunit;

namespace MediaEscolar.Tests;

public class CalculadoraTests
{
    [Fact]
    public void CalcularMedia_DeveRetornarMediaAritmeticaDasDuasNotas()
    {
        var media = Calculadora.CalcularMedia(8, 6);
        Assert.Equal(7, media);
    }

    [Fact]
    public void CalcularSituacao_DeveRetornarAprovadoQuandoMediaMaiorOuIgualASeis()
    {
        var situacao = Calculadora.CalcularSituacao(6.0);
        Assert.Equal("Aprovado", situacao);
    }

    [Fact]
    public void CalcularSituacao_DeveRetornarReprovadoQuandoMediaMenorQueSeis()
    {
        var situacao = Calculadora.CalcularSituacao(5.9);
        Assert.Equal("Reprovado", situacao);
    }

    [Fact]
    public void ValidarNota_DeveLancarExcecaoQuandoNotaForMaiorQueDez()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculadora.ValidarNota(10.1));
    }

    [Fact]
    public void ValidarNota_DeveLancarExcecaoQuandoNotaForMenorQueZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Calculadora.ValidarNota(-0.1));
    }

    [Fact]
    public void CalcularSituacao_RegressaoDaNotaDeCorteSeis()
    {
        Assert.Equal("Aprovado", Calculadora.CalcularSituacao(6.0));
        Assert.Equal("Reprovado", Calculadora.CalcularSituacao(5.99));
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `dotnet test tests/MediaEscolar.Tests`
Expected: FAIL with a compiler error such as `The type or namespace name 'Calculadora' does not exist in the namespace 'MediaEscolar.Api.Models'`.

- [ ] **Step 3: Implement the minimal code to make the tests pass**

Create `src/MediaEscolar.Api/Models/Calculadora.cs`:

```csharp
namespace MediaEscolar.Api.Models;

public static class Calculadora
{
    public const double NotaMinima = 0;
    public const double NotaMaxima = 10;
    public const double MediaAprovacao = 6.0;

    public static void ValidarNota(double nota)
    {
        if (nota < NotaMinima || nota > NotaMaxima)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nota),
                $"Nota deve estar entre {NotaMinima} e {NotaMaxima}.");
        }
    }

    public static double CalcularMedia(double nota1, double nota2) => (nota1 + nota2) / 2;

    public static string CalcularSituacao(double media) =>
        media >= MediaAprovacao ? "Aprovado" : "Reprovado";
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/MediaEscolar.Tests`
Expected: PASS — output ends with `Passed!` and `Failed: 0, Passed: 6` (6 tests, all green).

- [ ] **Step 5: Commit**

```bash
git add tests/MediaEscolar.Tests/CalculadoraTests.cs src/MediaEscolar.Api/Models/Calculadora.cs
git commit -m "Add Calculadora business logic with unit and regression tests"
```

---

## Task 4: TDD — Aluno model and Minimal API endpoints

**Files:**
- Create: `tests/MediaEscolar.Tests/ApiIntegrationTests.cs`
- Create: `src/MediaEscolar.Api/Models/Aluno.cs`
- Modify: `src/MediaEscolar.Api/Program.cs` (replace template content entirely)

- [ ] **Step 1: Write the failing integration test**

Create `tests/MediaEscolar.Tests/ApiIntegrationTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run the tests to verify the new one fails**

Run: `dotnet test tests/MediaEscolar.Tests`
Expected: FAIL — `FluxoCompleto_CadastrarRegistrarNotasEConsultar_DeveRetornarMediaESituacaoCorretas` fails (the template's `Program.cs` has no `/alunos` route, so the POST returns 404 instead of 201). The 6 `CalculadoraTests` still pass.

- [ ] **Step 3: Create the Aluno model**

Create `src/MediaEscolar.Api/Models/Aluno.cs`:

```csharp
namespace MediaEscolar.Api.Models;

public class Aluno
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public double? Nota1 { get; set; }
    public double? Nota2 { get; set; }
}
```

- [ ] **Step 4: Replace Program.cs with the real endpoints**

Replace the entire contents of `src/MediaEscolar.Api/Program.cs` with:

```csharp
using MediaEscolar.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var alunos = new List<Aluno>();
var proximoId = 1;

app.MapPost("/alunos", (CadastrarAlunoRequest request) =>
{
    var aluno = new Aluno { Id = proximoId++, Nome = request.Nome };
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
```

- [ ] **Step 5: Run the tests to verify everything passes**

Run: `dotnet test tests/MediaEscolar.Tests`
Expected: PASS — `Failed: 0, Passed: 7` (6 `CalculadoraTests` + 1 integration test).

- [ ] **Step 6: Commit**

```bash
git add tests/MediaEscolar.Tests/ApiIntegrationTests.cs src/MediaEscolar.Api/Models/Aluno.cs src/MediaEscolar.Api/Program.cs
git commit -m "Implement Aluno model and /alunos Minimal API endpoints"
```

---

## Task 5: Dockerfile and manual container verification

**Files:**
- Create: `Dockerfile`

- [ ] **Step 1: Write the Dockerfile**

Create `Dockerfile` at the repo root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/MediaEscolar.Api/MediaEscolar.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MediaEscolar.Api.dll"]
```

- [ ] **Step 2: Build the image**

Run: `docker build -t media-escolar .`
Expected: build completes with `=> => naming to docker.io/library/media-escolar` (or equivalent "successfully tagged" message) and no errors.

- [ ] **Step 3: Run the container**

Run: `docker run --rm -p 8080:8080 --name media-escolar-test -d media-escolar`
Expected: a container ID is printed.

- [ ] **Step 4: Verify the API responds**

In PowerShell:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:8080/alunos -ContentType "application/json" -Body '{"nome":"Teste"}'
```

Expected: a JSON object with `id: 1`, `nome: "Teste"`, `nota1: null`, `nota2: null`.

- [ ] **Step 5: Stop the container**

Run: `docker stop media-escolar-test`
Expected: the container name is printed back, confirming it stopped.

- [ ] **Step 6: Commit**

```bash
git add Dockerfile
git commit -m "Add multi-stage Dockerfile for MediaEscolar.Api"
```

---

## Task 6: GitHub Actions CI/CD pipeline

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Step 1: Write the workflow file**

Create `.github/workflows/ci.yml`:

```yaml
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build da imagem Docker
        run: docker build -t media-escolar .

      - name: Restaurar dependencias
        run: dotnet restore MediaEscolar.sln

      - name: Analise estatica (dotnet format)
        run: dotnet format MediaEscolar.sln --verify-no-changes

      - name: Executar testes com cobertura
        run: dotnet test MediaEscolar.sln --logger "trx;LogFileName=test-results.trx" --collect:"XPlat Code Coverage" --results-directory ./TestResults

      - name: Publicar evidencias (resultados de teste e cobertura)
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: evidencias-pipeline
          path: ./TestResults
```

- [ ] **Step 2: Run `dotnet format` locally so the lint step doesn't fail in CI**

Run: `dotnet format MediaEscolar.sln`
Expected: the command completes (it may rewrite some files to match default style rules).

- [ ] **Step 3: Verify formatting is now clean**

Run: `dotnet format MediaEscolar.sln --verify-no-changes`
Expected: exits with code 0 and no output listing files that need formatting. If it lists files, run Step 2 again.

- [ ] **Step 4: Re-run the full test suite after formatting**

Run: `dotnet test MediaEscolar.sln`
Expected: PASS — `Failed: 0, Passed: 7`.

- [ ] **Step 5: Commit**

```bash
git add .github/workflows/ci.yml
git diff --name-only | xargs -r git add
git commit -m "Add GitHub Actions CI/CD pipeline"
```

---

## Task 7: Push to GitHub and verify the pipeline runs

**This task pushes to the remote repository (`origin`). Confirm with the user before pushing.**

**Files:** none (no new files; verification task)

- [ ] **Step 1: Confirm with the user before pushing**

Ask: "Posso enviar (`git push`) os commits para o `origin` (`enzoblousa/RED_Ensino_Domiciliar`) no GitHub agora, para que a pipeline do GitHub Actions rode?" Wait for explicit confirmation.

- [ ] **Step 2: Push to the remote**

Run: `git push -u origin main`
Expected: branch `main` is created on `origin` and commits are uploaded.

- [ ] **Step 3: Check the workflow run**

Run: `gh run list --limit 1`
Expected: one row showing the `CI` workflow, branch `main`, status `in_progress` or `completed`.

- [ ] **Step 4: Wait for and inspect the result**

Run: `gh run watch $(gh run list --limit 1 --json databaseId --jq '.[0].databaseId')`
Expected: the run finishes with conclusion `success`. If it fails, run `gh run view --log-failed` to see which step failed, fix it, commit, push, and repeat this task.

- [ ] **Step 5: Capture the run URL for the README evidence section**

Run: `gh run list --limit 1 --json url --jq '.[0].url'`
Expected: a URL like `https://github.com/enzoblousa/RED_Ensino_Domiciliar/actions/runs/<id>`. Save this URL — it's needed in Task 9.

---

## Task 8: Simulate a failure, document it, and fix it

**Files:**
- Modify (temporarily): `src/MediaEscolar.Api/Models/Calculadora.cs`
- Create: `evidencias/falha-teste.txt`
- Create: `evidencias/correcao-teste.txt`

- [ ] **Step 1: Break the approval rule**

In `src/MediaEscolar.Api/Models/Calculadora.cs`, change:

```csharp
    public static string CalcularSituacao(double media) =>
        media >= MediaAprovacao ? "Aprovado" : "Reprovado";
```

to:

```csharp
    public static string CalcularSituacao(double media) =>
        media > MediaAprovacao ? "Aprovado" : "Reprovado";
```

- [ ] **Step 2: Run the tests and capture the failure**

Run: `dotnet test tests/MediaEscolar.Tests --logger "console;verbosity=detailed"`
Expected: FAIL — `Failed: 2, Passed: 5`. `CalcularSituacao_DeveRetornarAprovadoQuandoMediaMaiorOuIgualASeis` and `CalcularSituacao_RegressaoDaNotaDeCorteSeis` fail because `CalcularSituacao(6.0)` now returns `"Reprovado"` instead of `"Aprovado"`. The other 5 tests (including `FluxoCompleto_...`, whose average of 7.0 is still above the boundary either way) keep passing.

- [ ] **Step 3: Save the failure evidence**

Create the `evidencias/` directory and save the full terminal output from Step 2 into `evidencias/falha-teste.txt`. Include at minimum the lines showing `Failed!` and the names of the failing tests with their assertion messages (e.g. `Assert.Equal() Failure: Expected: Aprovado, Actual: Reprovado`).

- [ ] **Step 4: Revert the bug**

Change `Calculadora.cs` back to:

```csharp
    public static string CalcularSituacao(double media) =>
        media >= MediaAprovacao ? "Aprovado" : "Reprovado";
```

- [ ] **Step 5: Run the tests and capture the fix**

Run: `dotnet test tests/MediaEscolar.Tests --logger "console;verbosity=detailed"`
Expected: PASS — `Failed: 0, Passed: 7`. Save this output into `evidencias/correcao-teste.txt`.

- [ ] **Step 6: Commit**

```bash
git add evidencias/falha-teste.txt evidencias/correcao-teste.txt
git commit -m "Document simulated test failure and fix as pipeline evidence"
```

Note: `Calculadora.cs` itself has no net change (it was broken and then reverted to its original state), so there is nothing to commit for that file in this task.

---

## Task 9: Write the README

**Files:**
- Create: `README.md`

- [ ] **Step 1: Write the README**

Create `README.md` at the repo root, following the structure required by `DocumentoProfessor/red-enzo.pdf`:

```markdown
# API de Média Escolar

## Descrição
API simples para cadastro de alunos, registro de duas notas e cálculo automático da média e da situação (Aprovado/Reprovado). Construída como atividade da disciplina Testes e Qualidade de Software, demonstrando Docker, testes automatizados e uma esteira CI/CD.

## Tecnologias utilizadas
- Linguagem: C# / .NET 8
- Framework: ASP.NET Core Minimal API
- Ferramenta de testes: xUnit, coverlet (cobertura), Microsoft.AspNetCore.Mvc.Testing (testes de integração)
- Docker: Dockerfile multi-stage (sdk:8.0 para build, aspnet:8.0 para runtime)
- CI/CD: GitHub Actions

## Funcionalidades
- Cadastrar aluno (`POST /alunos`)
- Registrar duas notas de um aluno, validadas entre 0 e 10 (`POST /alunos/{id}/notas`)
- Consultar aluno com média e situação calculadas (`GET /alunos/{id}`)
- Listar todos os alunos cadastrados (`GET /alunos`)

## Como executar com Docker

### Build da imagem
docker build -t media-escolar .

### Execução da aplicação
docker run -p 8080:8080 media-escolar

A API fica disponível em `http://localhost:8080`.

## Como executar os testes

Localmente (sem Docker), com o SDK do .NET instalado:

dotnet test MediaEscolar.sln

## Pipeline CI/CD
A esteira (`.github/workflows/ci.yml`) executa, a cada push/PR para `main`:
1. Checkout do código (`actions/checkout@v4`)
2. Setup do .NET (`actions/setup-dotnet@v4`)
3. Build da imagem Docker (`docker build`)
4. Análise estática (`dotnet format --verify-no-changes`)
5. Execução dos testes com cobertura (`dotnet test --collect:"XPlat Code Coverage"`)
6. Publicação de evidências (relatório de testes `.trx` e cobertura `coverage.cobertura.xml` como artifacts)

## Tipos de testes implementados
- **Testes unitários** (`CalculadoraTests.cs`): cálculo da média, aprovação, reprovação, validação de notas inválidas.
- **Teste de regressão** (`CalculadoraTests.cs`): trava o valor exato da nota de corte (6.0) para a regra de aprovação.
- **Teste de integração/API** (`ApiIntegrationTests.cs`): fluxo completo via HTTP real (`WebApplicationFactory`) — cadastra aluno, registra notas, consulta média e situação.

## Evidências
- Execução da pipeline no GitHub Actions: <COLAR_URL_DO_RUN_CAPTURADA_NA_TASK_7>
- Log de testes passando localmente: ver `evidencias/correcao-teste.txt`

## Falha simulada e correção
Para demonstrar que a esteira detecta defeitos, a regra de aprovação foi temporariamente alterada de `média >= 6` para `média > 6` em `Calculadora.cs`. Isso quebrou dois testes que verificam o caso de borda (média exatamente 6.0): o teste de aprovação no limite e o teste de regressão da nota de corte. O log completo da falha está em `evidencias/falha-teste.txt`. Após reverter a alteração, os testes voltaram a passar — log completo em `evidencias/correcao-teste.txt`.

## Conclusão
O uso de Docker garante que a aplicação roda da mesma forma em qualquer máquina, eliminando o problema de "funciona na minha máquina". A esteira de CI/CD automatiza build, análise estática e testes a cada mudança no código, detectando regressões (como a simulada acima) antes que cheguem a produção. Os testes automatizados (unitários, de regressão e de integração) garantem que a regra de negócio mais importante do sistema — o cálculo da média e a decisão de aprovação — continue correta ao longo do tempo.
```

- [ ] **Step 2: Fill in the evidence URL**

Replace `<COLAR_URL_DO_RUN_CAPTURADA_NA_TASK_7>` with the actual GitHub Actions run URL captured in Task 7, Step 5.

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "Add project README with usage, pipeline, and evidence documentation"
```

---

## Task 10: Final push and pipeline confirmation

**This task pushes to the remote repository (`origin`). Confirm with the user before pushing.**

**Files:** none (verification task)

- [ ] **Step 1: Confirm with the user before pushing**

Ask: "Posso enviar o commit final (README + evidências) para o `origin/main`?" Wait for explicit confirmation.

- [ ] **Step 2: Push**

Run: `git push origin main`
Expected: commits uploaded without errors.

- [ ] **Step 3: Verify the final pipeline run is green**

Run: `gh run list --limit 1`
Expected: latest run for `main` shows conclusion `success`.

- [ ] **Step 4: Add the status badge to the README**

Run: `gh repo view --json nameWithOwner --jq '.nameWithOwner'` to confirm the owner/repo string, then add this line right under the `# API de Média Escolar` title in `README.md`:

```markdown
![CI](https://github.com/enzoblousa/RED_Ensino_Domiciliar/actions/workflows/ci.yml/badge.svg)
```

- [ ] **Step 5: Commit and push the badge**

```bash
git add README.md
git commit -m "Add CI status badge to README"
git push origin main
```
