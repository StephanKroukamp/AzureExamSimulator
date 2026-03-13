using AzureExamSimulator.Models;

namespace AzureExamSimulator.Services;

public class ExamService
{
    public Exam? CurrentExam { get; private set; }
    public ExamConfig? Config { get; private set; }
    public int CurrentQuestionIndex { get; private set; }
    public List<string[]> UserAnswers { get; private set; } = [];
    public bool IsExamActive { get; private set; }
    public bool IsExamFinished { get; private set; }
    public DateTime? StartTime { get; private set; }
    public bool HasSubmittedCurrentAnswer { get; private set; }

    public TimeSpan Elapsed => StartTime.HasValue
        ? DateTime.UtcNow - StartTime.Value
        : TimeSpan.Zero;

    public TimeSpan? TimeRemaining
    {
        get
        {
            if (Config is null || Config.TimeLimitMinutes <= 0 || !StartTime.HasValue)
                return null;
            var remaining = TimeSpan.FromMinutes(Config.TimeLimitMinutes) - Elapsed;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }

    public bool IsTimeExpired => TimeRemaining.HasValue && TimeRemaining.Value <= TimeSpan.Zero;

    public Question? CurrentQuestion =>
        CurrentExam is not null && CurrentQuestionIndex >= 0 && CurrentQuestionIndex < CurrentExam.Questions.Count
            ? CurrentExam.Questions[CurrentQuestionIndex]
            : null;

    public int TotalQuestions => CurrentExam?.Questions.Count ?? 0;

    public string[] GetCurrentAnswer()
    {
        if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < UserAnswers.Count)
            return UserAnswers[CurrentQuestionIndex];
        return [];
    }

    public void StartExam(Exam exam, ExamConfig config)
    {
        var validQuestions = QuestionValidator.GetValidQuestions(exam.Questions);

        var questionsToUse = validQuestions;
        if (config.QuestionCount < validQuestions.Count)
        {
            var rng = new Random();
            questionsToUse = validQuestions.OrderBy(_ => rng.Next()).Take(config.QuestionCount).ToList();
        }

        CurrentExam = new Exam
        {
            ExamCode = exam.ExamCode,
            ExamTitle = exam.ExamTitle,
            Questions = questionsToUse
        };

        Config = config;
        CurrentQuestionIndex = 0;
        UserAnswers = questionsToUse.Select(_ => Array.Empty<string>()).ToList();
        IsExamActive = true;
        IsExamFinished = false;
        HasSubmittedCurrentAnswer = false;
        StartTime = DateTime.UtcNow;
    }

    public void SubmitAnswer(string[] selectedAnswers)
    {
        if (!IsExamActive || CurrentQuestionIndex < 0 || CurrentQuestionIndex >= UserAnswers.Count)
            return;

        UserAnswers[CurrentQuestionIndex] = selectedAnswers;
        HasSubmittedCurrentAnswer = true;
    }

    public void GoToQuestion(int index)
    {
        if (CurrentExam is null || index < 0 || index >= CurrentExam.Questions.Count)
            return;

        CurrentQuestionIndex = index;
        HasSubmittedCurrentAnswer = UserAnswers[index].Length > 0;
    }

    public bool NextQuestion()
    {
        if (CurrentExam is null || CurrentQuestionIndex >= CurrentExam.Questions.Count - 1)
            return false;

        CurrentQuestionIndex++;
        HasSubmittedCurrentAnswer = UserAnswers[CurrentQuestionIndex].Length > 0;
        return true;
    }

    public bool PreviousQuestion()
    {
        if (CurrentQuestionIndex <= 0)
            return false;

        CurrentQuestionIndex--;
        HasSubmittedCurrentAnswer = UserAnswers[CurrentQuestionIndex].Length > 0;
        return true;
    }

    public void SkipQuestion()
    {
        if (!IsExamActive || CurrentQuestionIndex < 0 || CurrentQuestionIndex >= UserAnswers.Count)
            return;

        UserAnswers[CurrentQuestionIndex] = [];
        HasSubmittedCurrentAnswer = false;
        NextQuestion();
    }

    public void FinishExam()
    {
        IsExamActive = false;
        IsExamFinished = true;
    }

    public bool IsAnswerCorrect(int questionIndex)
    {
        if (CurrentExam is null || questionIndex < 0 || questionIndex >= CurrentExam.Questions.Count)
            return false;

        return AreAnswersCorrect(CurrentExam.Questions[questionIndex], UserAnswers[questionIndex]);
    }

    public bool IsCurrentAnswerCorrect()
    {
        return IsAnswerCorrect(CurrentQuestionIndex);
    }

    public ExamResult CalculateResult()
    {
        if (CurrentExam is null)
            return new ExamResult();

        var result = new ExamResult
        {
            TotalQuestions = CurrentExam.Questions.Count,
            TimeTaken = Elapsed
        };

        for (var i = 0; i < CurrentExam.Questions.Count; i++)
        {
            var question = CurrentExam.Questions[i];
            var userAnswer = UserAnswers[i];
            var topic = string.IsNullOrEmpty(question.Topic) ? "General" : question.Topic;

            if (!result.TopicBreakdown.ContainsKey(topic))
                result.TopicBreakdown[topic] = (0, 0);

            var (total, correct) = result.TopicBreakdown[topic];

            if (userAnswer.Length == 0)
            {
                result.Skipped++;
                result.TopicBreakdown[topic] = (total + 1, correct);
            }
            else if (AreAnswersCorrect(question, userAnswer))
            {
                result.Correct++;
                result.Answered++;
                result.TopicBreakdown[topic] = (total + 1, correct + 1);
            }
            else
            {
                result.Incorrect++;
                result.Answered++;
                result.TopicBreakdown[topic] = (total + 1, correct);
            }
        }

        return result;
    }

    public void Reset()
    {
        CurrentExam = null;
        Config = null;
        CurrentQuestionIndex = 0;
        UserAnswers = [];
        IsExamActive = false;
        IsExamFinished = false;
        HasSubmittedCurrentAnswer = false;
        StartTime = null;
    }

    private static bool AreAnswersCorrect(Question question, string[] userAnswers)
    {
        if (userAnswers.Length == 0) return false;

        var ua = userAnswers
            .Where(a => a is not null)
            .Select(a => a.Trim().ToUpperInvariant())
            .OrderBy(a => a)
            .ToList();

        var ca = question.CorrectAnswers
            .Select(a => a.Trim().ToUpperInvariant())
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        return ua.SequenceEqual(ca);
    }
}
