namespace AzureExamSimulator.Models;

public record ExamConfig(
    bool IsPracticeMode,
    int TimeLimitMinutes,
    int QuestionCount,
    List<string>? SelectedTopics = null
);
