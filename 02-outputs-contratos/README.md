# Contratos de dados em workflows com LangChain .NET

Mesmo workflow sequencial da aula 1.3 (evidencias -> hipotese -> plano), agora com contratos de
entrada e saida (aula 1.4) e exposto como API.

- `Contratos.cs`: records com validacao (equivalem aos modelos Pydantic): `AlertaEntrada`,
  `Evidencias`, `Hipotese`, `PlanoVerificacao` e `AnaliseIncidente`.
- `Workflow.cs`: tres chains com saida estruturada (JSON Schema). Cada objeto vira JSON antes de
  ser a entrada da etapa seguinte.
- `Program.cs`: `POST /analises`. Entrada invalida e rejeitada com 400 antes de chamar o modelo.

## Executar

```powershell
$env:GOOGLE_API_KEY = "sua-chave-aqui"
dotnet run --project 02-outputs-contratos
curl.exe -X POST http://localhost:5000/analises -H "Content-Type: application/json" -d "@02-outputs-contratos/entrada.json"
```
