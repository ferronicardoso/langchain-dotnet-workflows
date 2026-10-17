# Routing workflow com LangChain .NET

Triagem condicional (aula 1.5): um roteador classifica a solicitacao e um branch executa somente a
tratativa da rota escolhida, cada uma com seu provider e modelo.

| Rota | Provider | Modelo |
|---|---|---|
| roteador | OpenAI | `gpt-5.6-luna` |
| `pipeline` | Anthropic | `claude-sonnet-5` |
| `infraestrutura` | OpenAI | `gpt-5.6-sol` |
| `release` | Anthropic | `claude-haiku-4-5-20251001` |
| `fallback` | aplicacao (nenhum modelo) | - |

O LangChain .NET nao tem `RunnableBranch`; `Branch<TIn, TOut>` (em `Workflow.cs`) tem o mesmo
comportamento: avalia as condicoes em ordem, executa so a primeira verdadeira e usa o padrao se
nenhuma servir.

## Executar

```powershell
$env:OPENAI_API_KEY = "sua-chave-aqui"
$env:ANTHROPIC_API_KEY = "sua-chave-aqui"
dotnet run --project 03-routing-workflow
curl.exe -X POST http://localhost:5000/triagens -H "Content-Type: application/json" -d "@03-routing-workflow/entradas/falha-pipeline.json"
```

Entradas de exemplo em `entradas/`. A classificacao e feita por modelo: a rota escolhida e o texto
das respostas podem variar a cada execucao.
