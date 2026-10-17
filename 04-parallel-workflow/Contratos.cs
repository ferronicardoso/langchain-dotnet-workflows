using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ParallelApp;

// Contratos do workflow paralelo (equivalem a contratos.py da aula 1.6).

public record Incidente(
    [property: Required] string Titulo,
    [property: Required] string Descricao);

public enum Perspectiva { Pipeline, Infraestrutura, Aplicacao }

public enum Severidade { Baixa, Media, Alta, Critica }

// No lugar da rota, a Analise traz a perspectiva que a produziu.
public record Analise(
    Perspectiva Perspectiva,
    List<string> Evidencias,
    string Hipotese,
    Severidade Severidade);

// Um campo por ramo do fan-out.
public record AnalisesParalelas(Analise Pipeline, Analise Infraestrutura, Analise Aplicacao);

// Saída do fan-in.
public record Consolidacao(
    string Resumo,
    List<string> Convergencias,
    List<string> Divergencias,
    List<string> ProximosPassos);

public record Resultado(AnalisesParalelas Analises, Consolidacao Consolidacao);

// JSON em snake_case e enums em minúsculas, como nos contratos Pydantic do curso.
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Padrao = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static readonly JsonSerializerOptions Indentado = new(Padrao) { WriteIndented = true };
}
