# Workflow paralelo com LangChain .NET

Execucao paralela (aula 1.6): o mesmo incidente e analisado por tres perspectivas independentes
(pipeline, infraestrutura e aplicacao) e uma consolidacao reune os resultados.

```text
                  +--> pipeline ---------+
incidente --------+--> infraestrutura ---+--> consolidacao
   (fan-out)      +--> aplicacao --------+      (fan-in)
```

So pode ser paralelo o que nao depende do resultado de outra etapa. Aqui as tres analises
dependem apenas do incidente; a consolidacao depende das tres e por isso roda depois.

O LangChain .NET nao tem `RunnableParallel`; o fan-out e um `Task.WhenAll` sobre as tres chains
(`Workflow.cs`). A resposta traz as analises individuais e a consolidacao.

## Executar

```powershell
$env:OPENAI_API_KEY = "sua-chave-aqui"
dotnet run --project 04-parallel-workflow
curl.exe -X POST http://localhost:5000/analises -H "Content-Type: application/json" -d "@04-parallel-workflow/entradas/incidente.json"
```
