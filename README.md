# API de Média Escolar

![CI](https://github.com/enzoblousa/RED_Ensino_Domiciliar/actions/workflows/ci.yml/badge.svg)

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
```
docker build -t media-escolar .
```

### Execução da aplicação
```
docker run -p 8080:8080 media-escolar
```

A API fica disponível em `http://localhost:8080`.

## Como executar os testes

Localmente (sem Docker), com o SDK do .NET 8 instalado:

```
dotnet test MediaEscolar.sln
```

## Pipeline CI/CD
A esteira (`.github/workflows/ci.yml`) executa, a cada push/PR para `main`:
1. Checkout do código (`actions/checkout@v4`)
2. Setup do .NET (`actions/setup-dotnet@v4`)
3. Build da imagem Docker (`docker build`)
4. Análise estática (`dotnet format --verify-no-changes --report ./TestResults`, gerando `format-report.json`)
5. Execução dos testes com cobertura (`dotnet test --collect:"XPlat Code Coverage"`)
6. Captura automática de screenshot: sobe o container da imagem buildada, executa o fluxo cadastro → notas → consulta via `curl` e usa o Playwright (Chromium headless) para fotografar o JSON final em `screenshot-execucao.png`
7. Publicação de evidências (relatório do linter, relatório de testes, cobertura e screenshot como artifacts)

## Tipos de testes implementados
- **Testes unitários** (`CalculadoraTests.cs`): cálculo da média, aprovação, reprovação, validação de notas inválidas.
- **Teste de regressão** (`CalculadoraTests.cs`): trava o valor exato da nota de corte (6.0) para a regra de aprovação.
- **Teste de integração/API** (`ApiIntegrationTests.cs`): fluxo completo via HTTP real (`WebApplicationFactory`) — cadastra aluno, registra notas, consulta média e situação.

## Evidências
- Execução da pipeline no GitHub Actions: https://github.com/enzoblousa/RED_Ensino_Domiciliar/actions/runs/27924637273
- Log de testes passando localmente: ver `evidencias/correcao-teste.txt`
- Relatório do linter: `format-report.json`, gerado pelo `dotnet format --report` e publicado como artifact `evidencias-pipeline` em cada execução
- Screenshot da execução: `screenshot-execucao.png`, gerado automaticamente em cada execução (sem intervenção manual) e publicado no artifact `evidencias-pipeline` — baixável na página da run correspondente em GitHub Actions

## Falha simulada e correção
Para demonstrar que a esteira detecta defeitos, a regra de aprovação foi temporariamente alterada de `média >= 6` para `média > 6` em `Calculadora.cs`. Isso quebrou dois testes que verificam o caso de borda (média exatamente 6.0): o teste de aprovação no limite e o teste de regressão da nota de corte. O log completo da falha está em `evidencias/falha-teste.txt`. Após reverter a alteração, os testes voltaram a passar — log completo em `evidencias/correcao-teste.txt`.

## Conclusão
O uso de Docker garante que a aplicação roda da mesma forma em qualquer máquina, eliminando o problema de "funciona na minha máquina". A esteira de CI/CD automatiza build, análise estática e testes a cada mudança no código, detectando regressões (como a simulada acima) antes que cheguem a produção. Os testes automatizados (unitários, de regressão e de integração) garantem que a regra de negócio mais importante do sistema — o cálculo da média e a decisão de aprovação — continue correta ao longo do tempo.
