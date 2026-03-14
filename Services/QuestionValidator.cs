using AzureExamSimulator.Models;

namespace AzureExamSimulator.Services;

public static class QuestionValidator
{
    public static bool IsValid(Question question)
    {
        return question.Type switch
        {
            "Ordering" => ValidateOrdering(question),
            "Hotspot" => ValidateHotspot(question),
            _ => ValidateChoiceQuestion(question)
        };
    }

    private static bool ValidateChoiceQuestion(Question question)
    {
        if (question.Options.Count == 0)
            return false;

        if (question.CorrectAnswers.Count == 0)
            return false;

        foreach (var answer in question.CorrectAnswers)
        {
            var letter = answer.Trim().ToUpperInvariant();
            if (letter.Length == 1 && char.IsLetter(letter[0]))
            {
                var index = letter[0] - 'A';
                if (index < 0 || index >= question.Options.Count)
                    return false;
            }
        }

        return true;
    }

    private static bool ValidateOrdering(Question question)
    {
        if (question.Items is not { Count: > 0 })
            return false;

        if (question.CorrectOrder is not { Count: > 0 })
            return false;

        return question.CorrectOrder.Count == question.Items.Count;
    }

    private static bool ValidateHotspot(Question question)
    {
        if (question.Regions is not { Count: > 0 })
            return false;

        if (question.CorrectAnswers.Count == 0)
            return false;

        var regionIds = question.Regions.Select(r => r.Id).ToHashSet();
        return question.CorrectAnswers.All(a => regionIds.Contains(a));
    }

    public static List<Question> GetValidQuestions(List<Question> questions)
    {
        return questions.Where(IsValid).ToList();
    }

    public static int GetValidCount(List<Question> questions)
    {
        return questions.Count(IsValid);
    }
}
