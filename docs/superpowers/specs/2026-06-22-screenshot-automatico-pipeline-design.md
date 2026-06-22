# Design: Screenshot automático da execução na pipeline CI/CD

## Contexto

A Etapa 5 (Geração de evidências) exige, entre outros itens, um "screenshot da execução". Hoje esse screenshot só existe como print manual da página do GitHub Actions, comitado em `evidencias/EvidenciaFalha1.png`. Esse tipo de print (a UI do próprio Actions) não pode ser automatizado de dentro do job, pois o job não tem como capturar a tela da página que o exibe depois de terminar.

Como alternativa automatizável, este design adiciona um passo na pipeline que sobe a aplicação real (a partir da imagem Docker já buildada), exercita o fluxo de negócio via `curl` e captura, via navegador headless, o JSON final retornado pela API — uma evidência visual de que a aplicação empacotada está funcionando, gerada a cada execução sem intervenção manual.

## Escopo

- Novo passo no `.github/workflows/ci.yml`, entre "Executar testes com cobertura" e "Publicar evidências".
- Não altera o código da aplicação (`src/`) nem os testes existentes.
- **Substitui** o print manual: `evidencias/EvidenciaFalha1.png` é removido do repositório e do README; o screenshot automático passa a ser a única evidência de "screenshot da execução".
- O screenshot gerado é um artifact de cada execução (baixável na run), não é comitado no repositório — ou seja, a evidência deixa de ser um arquivo fixo no repo e passa a ser renovada a cada execução da pipeline.

## Fluxo do novo passo

1. Sobe o container a partir da imagem já buildada no passo "Build da imagem Docker":
   `docker run -d --name media-escolar -p 8080:8080 media-escolar`
2. Aguarda a API responder, fazendo retry de `curl http://localhost:8080/alunos` até obter sucesso (timeout curto, ex. 30s).
3. Exercita o fluxo de negócio via `curl`, replicando o cenário do teste de integração:
   - `POST /alunos` com `{ "nome": "Maria" }` → captura o `id` retornado.
   - `POST /alunos/{id}/notas` com `{ "nota1": 8.0, "nota2": 6.0 }`.
4. Usa Playwright via `npx` (Chromium headless; runner `ubuntu-latest` já tem Node.js, sem necessidade de novo projeto/dependência no repo) para abrir `http://localhost:8080/alunos/{id}` e salvar um print:
   `npx --yes playwright@1.48.0 screenshot http://localhost:8080/alunos/$ID ./TestResults/screenshot-execucao.png`
   (a instalação do browser Chromium é feita via `npx --yes playwright@1.48.0 install --with-deps chromium` antes do screenshot)
5. Encerra o container (`docker rm -f media-escolar`), independentemente de sucesso ou falha do passo (usar `if: always()` apenas no encerramento, ou `continue-on-error` controlado).

O arquivo `./TestResults/screenshot-execucao.png` cai na mesma pasta que já é publicada pelo passo "Publicar evidências" (`path: ./TestResults`), então esse passo não precisa de alteração.

## Tratamento de erros

- Se a API não responder dentro do timeout do passo 2, o passo falha explicitamente (sem fallback silencioso) — isso é um sinal real de que a imagem Docker não está funcionando, e deve quebrar a pipeline.
- O container é removido mesmo se o passo falhar, para não deixar processos orfãos em execuções futuras do runner (cada job roda em uma VM efêmera, mas a limpeza explícita é mais segura caso o runner reaproveite estado).

## Documentação (README)

Atualizar a seção "Evidências": remover a referência a `evidencias/EvidenciaFalha1.png` e descrever a nova fonte única de screenshot:
- Artifact `evidencias-pipeline` → `screenshot-execucao.png` — print automático do JSON da API em execução, gerado a cada run (não comitado, baixado da run).

## Fora de escopo

- Captura de tela da própria UI do GitHub Actions (não é tecnicamente possível de dentro do job — é exatamente o que este design substitui por uma evidência automatizável).
- Comitar automaticamente o screenshot gerado de volta no repositório (mantém o artifact apenas como anexo de cada run, evitando commits automáticos a cada execução).
- Swagger/OpenAPI UI (não está configurado no `Program.cs` atual; fora de escopo deste design).
