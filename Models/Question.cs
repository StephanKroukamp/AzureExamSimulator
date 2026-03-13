using System.Text.Json.Serialization;

namespace AzureExamSimulator.Models;

public class Question
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "SingleChoice";

    [JsonPropertyName("questionText")]
    public string QuestionText { get; set; } = "";

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = [];

    [JsonPropertyName("correctAnswers")]
    public List<string> CorrectAnswers { get; set; } = [];

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = "";
}
