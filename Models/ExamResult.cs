namespace AzureExamSimulator.Models;

public class ExamResult
{
    public int TotalQuestions { get; set; }
    public int Answered { get; set; }
    public int Correct { get; set; }
    public int Partial { get; set; }
    public int Incorrect { get; set; }
    public int Skipped { get; set; }
    public double EarnedScore { get; set; }
    public Dictionary<string, (int Total, double Correct)> TopicBreakdown { get; set; } = new();
    public TimeSpan TimeTaken { get; set; }

    public double Percentage => TotalQuestions > 0 ? EarnedScore / TotalQuestions * 100 : 0;
    public int ScoreOutOf1000 => TotalQuestions > 0 ? (int)(EarnedScore / TotalQuestions * 1000) : 0;
    public bool Passed => ScoreOutOf1000 >= 700;
}
