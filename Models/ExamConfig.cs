namespace AzureExamSimulator.Models;

public record ExamConfig(
    bool IsPracticeMode,
    int TimeLimitMinutes,
    int QuestionCount,
    bool IsStudyMode = false,
    List<string>? SelectedTopics = null,
    bool Randomize = false
);
