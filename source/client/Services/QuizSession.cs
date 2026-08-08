using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Client.Services;

public sealed class QuizSession
{
    public List<Question> Questions { get; private set; } = new();
    public List<GradeResult?> Results { get; private set; } = new();
    public int CurrentIndex { get; set; }

    public void Start(List<Question> questions)
    {
        Questions = questions;
        Results = questions.Select(_ => (GradeResult?)null).ToList();
        CurrentIndex = 0;
    }

    public void Record(int i, GradeResult r) => Results[i] = r;
    public double Score => Results.Where(r => r is not null).Sum(r => r!.PointsAwarded);
    public double MaxScore => Questions.Sum(q => q.Points);
    public bool IsStarted => Questions.Count > 0;
}
