# LangChain .NET Workflows

Exemplos de workflows com modelos de IA em C# (.NET 10) usando o [LangChain .NET](https://github.com/tryAGI/LangChain).
O projeto e uma versao em .NET do repositorio da aula
[KubeDev/langchain-workflows](https://github.com/KubeDev/langchain-workflows), do modulo
"Construcao de Agentes e Fundamentos de AIOps" (Python). Cada pasta corresponde a uma aula do curso.

## Projetos

| Projeto | Aula | O que mostra | Tipo | Provider |
|---|---|---|---|---|
| [`01-workflow-sequencial`](01-workflow-sequencial) | 1.3 | Prompt chaining: alerta, evidencias, hipotese, plano | Console | Google (Gemini) |
| [`02-outputs-contratos`](02-outputs-contratos) | 1.4 | Contratos de entrada e saida, saida estruturada | API (`POST /analises`) | Google (Gemini) |
| [`03-routing-workflow`](03-routing-workflow) | 1.5 | Roteador e branch condicional, um modelo por rota | API (`POST /triagens`) | OpenAI e Anthropic |
| [`04-parallel-workflow`](04-parallel-workflow) | 1.6 | Fan-out em paralelo e consolidacao (fan-in) | API (`POST /analises`) | OpenAI |

Os quatro projetos sao independentes e autocontidos, como no repositorio original. Cada um tem
README proprio e exemplos de entrada.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Chaves de API dos provedores usados pelo projeto que for executar

| Variavel de ambiente | Usada em |
|---|---|
| `GOOGLE_API_KEY` | 01, 02 |
| `OPENAI_API_KEY` | 03, 04 |
| `ANTHROPIC_API_KEY` | 03 |

Nao grave chaves no codigo nem em arquivos versionados.

## Como executar

Defina as variaveis do projeto escolhido e rode a partir da raiz:

```powershell
$env:OPENAI_API_KEY = "sua-chave-aqui"
dotnet run --project 04-parallel-workflow
```

Os projetos de API sobem na porta padrao do ASP.NET (`http://localhost:5000`), entao rode um por vez.
Em outro terminal, envie um exemplo:

```powershell
curl.exe -X POST http://localhost:5000/analises -H "Content-Type: application/json" -d "@04-parallel-workflow/entradas/incidente.json"
```

O comando de cada projeto esta no README da pasta. Para compilar tudo:

```powershell
dotnet build langchain-dotnet-workflows.slnx
```

## Equivalencias com o LangChain Python

O curso usa Python. Estas sao as correspondencias adotadas aqui:

| Python (curso) | .NET (este repositorio) |
|---|---|
| `init_chat_model` | `IChatClient` (`Microsoft.Extensions.AI`) |
| `prompt \| model \| StrOutputParser()` | `Set(...) \| Template(...) \| LLM(...)` do LangChain .NET |
| `model.with_structured_output(Contrato)` | `ChatOptions.ResponseFormat` com JSON Schema, via `Cadeia.ExecutarEstruturadoAsync<T>` |
| Modelos Pydantic | `record` com atributos de validacao (`[Required]`, `[MinLength]`, `[Range]`) |
| `RunnableBranch` e `RunnableLambda` | `Branch<TIn, TOut>` (projeto 03) |
| `RunnableParallel` | `Task.WhenAll` (projeto 04) |
| FastAPI | ASP.NET Core minimal API |
| `python-dotenv` (`.env`) | variaveis de ambiente |

O LangChain .NET nao tem equivalente de `RunnableBranch` nem de `RunnableParallel`; por isso o projeto 03
traz a classe `Branch` e o 04 usa `Task.WhenAll`. A saida das APIs usa snake_case e enums em minusculas,
como os contratos do curso.

## Observacoes

- Os nomes dos modelos seguem o material do curso (por exemplo `gpt-5.6-luna` e `claude-sonnet-5`) e
  podem ser trocados por variaveis de ambiente (`MODELO` nos projetos 02 e 04; `MODELO_ROUTER`,
  `MODELO_PIPELINE`, `MODELO_INFRAESTRUTURA` e `MODELO_RELEASE` no 03), lidas no `Program.cs` de
  cada projeto. Confirme os identificadores disponiveis na sua conta.
- O texto gerado e a rota escolhida pelo roteador variam a cada execucao.
- As respostas dos modelos nao sao revalidadas apos o retorno; as regras dos contratos valem para a
  entrada da API.
