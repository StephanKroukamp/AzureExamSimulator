using System.Text.Json.Serialization;

namespace AzureExamSimulator.Models;

public class Exam
{
    [JsonPropertyName("examCode")]
    public string ExamCode { get; set; } = "";

    [JsonPropertyName("examTitle")]
    public string ExamTitle { get; set; } = "";

    [JsonPropertyName("questions")]
    public List<Question> Questions { get; set; } = [];
}
