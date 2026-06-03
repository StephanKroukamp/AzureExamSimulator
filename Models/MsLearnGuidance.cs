using System.Text.Json.Serialization;

namespace AzureExamSimulator.Models;

public class MsLearnGuidance
{
    [JsonPropertyName("primaryTopic")]
    public string PrimaryTopic { get; set; } = "";

    [JsonPropertyName("navigationPath")]
    public string NavigationPath { get; set; } = "";

    [JsonPropertyName("searchKeywords")]
    public List<string> SearchKeywords { get; set; } = [];

    [JsonPropertyName("lookFor")]
    public string LookFor { get; set; } = "";

    [JsonPropertyName("docsUrl")]
    public string? DocsUrl { get; set; }
}
