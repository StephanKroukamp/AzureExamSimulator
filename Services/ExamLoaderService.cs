using System.Text.Json;
using AzureExamSimulator.Models;

namespace AzureExamSimulator.Services;

public class ExamLoaderService
{
    private readonly string _dataPath;
    private List<Exam>? _cachedExams;

    public ExamLoaderService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "Data");
    }

    public List<Exam> LoadAllExams()
    {
        if (_cachedExams is not null)
            return _cachedExams;

        if (!Directory.Exists(_dataPath))
            return [];

        var exams = new List<Exam>();

        foreach (var file in Directory.GetFiles(_dataPath, "*.json").OrderBy(f => f))
        {
            var json = File.ReadAllText(file);
            var exam = JsonSerializer.Deserialize<Exam>(json);
            if (exam is not null && exam.Questions.Count > 0)
                exams.Add(exam);
        }

        _cachedExams = exams;
        return exams;
    }

    public Exam? LoadExam(string examCode)
    {
        var allExams = LoadAllExams();
        return allExams.FirstOrDefault(e =>
            e.ExamCode.Equals(examCode, StringComparison.OrdinalIgnoreCase));
    }
}
