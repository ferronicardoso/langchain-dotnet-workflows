# Workflow sequencial com LangChain .NET

Exemplo didatico de prompt chaining aplicado a um alerta operacional (aula 1.3).

```text
alerta -> evidencias -> hipotese -> plano de verificacao
```

A trajetoria e fixa: o codigo decide a ordem, o modelo nao escolhe etapas. Cada etapa e uma chain
do LangChain .NET: `Set(valor) | Template(mensagem) | LLM(modelo)`. A saida de uma etapa (string)
vira a entrada da proxima. Equivale a `prompt | model | StrOutputParser()` do LangChain Python.

## Executar

```powershell
$env:GOOGLE_API_KEY = "sua-chave-aqui"
dotnet run --project 01-workflow-sequencial
```

O alerta de entrada fica em `alerta.txt`.
