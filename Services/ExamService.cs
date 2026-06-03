using AzureExamSimulator.Models;

namespace AzureExamSimulator.Services;

public class ExamService
{
    public Exam? CurrentExam { get; private set; }
    public ExamConfig? Config { get; private set; }
    public int CurrentQuestionIndex { get; private set; }
    public List<string[]> UserAnswers { get; private set; } = [];
    public HashSet<int> FlaggedQuestions { get; private set; } = [];
    public bool IsExamActive { get; private set; }
    public bool IsExamFinished { get; private set; }
    public DateTime? StartTime { get; private set; }
    public bool HasSubmittedCurrentAnswer { get; private set; }

    public void ToggleFlag(int index)
    {
        if (!FlaggedQuestions.Remove(index)) FlaggedQuestions.Add(index);
    }

    public bool IsFlagged(int index) => FlaggedQuestions.Contains(index);

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

        if (config.SelectedTopics is { Count: > 0 })
        {
            validQuestions = validQuestions
                .Where(q => config.SelectedTopics.Contains(q.Topic))
                .ToList();
        }

        var rng = new Random();

        var questionsToUse = validQuestions;
        if (config.QuestionCount < validQuestions.Count)
            questionsToUse = validQuestions.OrderBy(_ => rng.Next()).Take(config.QuestionCount).ToList();

        if (config.Randomize)
            questionsToUse = ShufflePreservingCaseStudies(questionsToUse, rng);

        CurrentExam = new Exam
        {
            ExamCode = exam.ExamCode,
            ExamTitle = exam.ExamTitle,
            Questions = questionsToUse
        };

        Config = config;
        CurrentQuestionIndex = 0;
        UserAnswers = questionsToUse.Select(_ => Array.Empty<string>()).ToList();
        FlaggedQuestions = [];
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

    public bool IsCurrentAnswerCorrect() => IsAnswerCorrect(CurrentQuestionIndex);

    public double GetScore(int questionIndex)
    {
        if (CurrentExam is null || questionIndex < 0 || questionIndex >= CurrentExam.Questions.Count)
            return 0.0;
        return ScoreAnswer(CurrentExam.Questions[questionIndex], UserAnswers[questionIndex]);
    }

    public double GetCurrentScore() => GetScore(CurrentQuestionIndex);

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
            else
            {
                var score = ScoreAnswer(question, userAnswer);
                result.EarnedScore += score;
                result.Answered++;
                if (score >= 1.0)
                {
                    result.Correct++;
                    result.TopicBreakdown[topic] = (total + 1, correct + 1.0);
                }
                else if (score > 0.0)
                {
                    result.Partial++;
                    result.TopicBreakdown[topic] = (total + 1, correct + score);
                }
                else
                {
                    result.Incorrect++;
                    result.TopicBreakdown[topic] = (total + 1, correct);
                }
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
        FlaggedQuestions = [];
        IsExamActive = false;
        IsExamFinished = false;
        HasSubmittedCurrentAnswer = false;
        StartTime = null;
    }

    private static List<Question> ShufflePreservingCaseStudies(List<Question> questions, Random rng)
    {
        // Group consecutive questions sharing the same scenario into blocks,
        // then shuffle the blocks so case study questions stay together.
        var blocks = new List<List<Question>>();
        foreach (var q in questions)
        {
            if (!string.IsNullOrEmpty(q.Scenario) && blocks.Count > 0
                && !string.IsNullOrEmpty(blocks[^1][0].Scenario)
                && blocks[^1][0].Scenario == q.Scenario)
            {
                blocks[^1].Add(q);
            }
            else
            {
                blocks.Add([q]);
            }
        }

        return blocks.OrderBy(_ => rng.Next()).SelectMany(b => b).ToList();
    }

    private static bool AreAnswersCorrect(Question question, string[] userAnswers)
        => ScoreAnswer(question, userAnswers) >= 1.0;

    public static double ScoreAnswer(Question question, string[] userAnswers)
    {
        if (userAnswers.Length == 0) return 0.0;

        // Ordering / DragDrop: positional match rate
        if (question.Type is "Ordering" or "DragDrop" && question.CorrectOrder is { Count: > 0 })
        {
            var orderStr = userAnswers[0];
            if (string.IsNullOrEmpty(orderStr)) return 0.0;
            try
            {
                var userOrder = orderStr.Split(',').Select(int.Parse).ToList();
                if (userOrder.Count != question.CorrectOrder.Count) return 0.0;
                var matches = userOrder.Zip(question.CorrectOrder, (u, c) => u == c ? 1 : 0).Sum();
                return (double)matches / question.CorrectOrder.Count;
            }
            catch { return 0.0; }
        }

        // Choice / Hotspot
        var userSet = userAnswers
            .Where(a => a is not null)
            .Select(a => a.Trim().ToUpperInvariant())
            .ToHashSet();

        var correctSet = question.CorrectAnswers
            .Select(a => a.Trim().ToUpperInvariant())
            .ToHashSet();

        if (correctSet.Count == 0) return 0.0;

        // Single correct answer: binary
        if (correctSet.Count == 1)
            return userSet.SetEquals(correctSet) ? 1.0 : 0.0;

        // Multiple correct answers: partial credit based on correct picks only
        var intersection = userSet.Intersect(correctSet).Count();
        return (double)intersection / correctSet.Count;
    }
}
