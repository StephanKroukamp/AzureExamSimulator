using AzureExamSimulator.Models;

namespace AzureExamSimulator.Services;

public static class QuestionValidator
{
    public static bool IsValid(Question question)
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

    public static List<Question> GetValidQuestions(List<Question> questions)
    {
        return questions.Where(IsValid).ToList();
    }

    public static int GetValidCount(List<Question> questions)
    {
        return questions.Count(IsValid);
    }
}
