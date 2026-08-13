using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Domain.L0.Builders;

public sealed class QuestionBuilder
{
    private int _id = 1;
    private QuestionType _type = QuestionType.Single;
    private List<ContentBlock> _content = new List<ContentBlock> { new ContentBlock { Type = ContentBlockType.Text, Text = "Sample question?" } };
    private List<string> _category = new List<string> { "general" };
    private string _olympiad = "OIJ";
    private string _stage = "E1";
    private List<string> _options = new List<string> { "Option A", "Option B" };
    private List<string> _matchOptions = null;
    private List<string> _correctAnswer = new List<string> { "Option A" };
    private int _points = 1;
    private bool _partialCredit = false;
    private List<string> _algorithms = new List<string>();
    private int? _year = null;
    private int? _difficulty = null;
    private string _source = null;
    private string _sourceUrl = null;
    private string _sourceRaw = null;
    private string _explanationSource = null;
    private List<ContentBlock> _contentCpp = null;
    private List<ContentBlock> _explanation = null;

    public static QuestionBuilder AQuestion()
    {
        return new QuestionBuilder();
    }

    public static QuestionBuilder AFullyPopulatedQuestion()
    {
        QuestionBuilder builder = new QuestionBuilder();
        builder._algorithms       = new List<string> { "sorting" };
        builder._year             = 2024;
        builder._difficulty       = 3;
        builder._source           = "OIJ-2024-E1";
        builder._sourceUrl        = "https://example.com/source";
        builder._sourceRaw        = "raw source text";
        builder._explanationSource = "Explanation from textbook";
        builder._contentCpp       = new List<ContentBlock> { new ContentBlock { Type = ContentBlockType.Code, Text = "int main() {}" } };
        builder._matchOptions     = new List<string> { "Match A", "Match B" };
        builder._explanation      = new List<ContentBlock> { new ContentBlock { Type = ContentBlockType.Text, Text = "Because..." } };

        builder._content = new List<ContentBlock>
        {
            new ContentBlock { Type = ContentBlockType.Text,  Text = "Question with żółty ₁₆ and 2² and \U0001D465?" },
            new ContentBlock { Type = ContentBlockType.Image, File = "img/q1.png", Alt = "Diagram showing a graph" }
        };

        return builder;
    }

    public QuestionBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public QuestionBuilder WithType(QuestionType type)
    {
        _type = type;
        return this;
    }

    public QuestionBuilder WithContent(params ContentBlock[] blocks)
    {
        _content = new List<ContentBlock>(blocks);
        return this;
    }

    public QuestionBuilder WithoutContent()
    {
        _content = null;
        return this;
    }

    public QuestionBuilder WithOptions(params string[] options)
    {
        _options = new List<string>(options);
        return this;
    }

    public QuestionBuilder WithoutOptions()
    {
        _options = null;
        return this;
    }

    public QuestionBuilder WithMatchOptions(params string[] matchOptions)
    {
        _matchOptions = new List<string>(matchOptions);
        return this;
    }

    public QuestionBuilder WithCorrectAnswer(params string[] values)
    {
        _correctAnswer = new List<string>(values);
        return this;
    }

    public QuestionBuilder WithPoints(int points)
    {
        _points = points;
        return this;
    }

    public QuestionBuilder WithPartialCredit(bool partialCredit)
    {
        _partialCredit = partialCredit;
        return this;
    }

    public Question Build()
    {
        return new Question
        {
            Id              = _id,
            Type            = _type,
            Content         = _content       != null ? new List<ContentBlock>(_content) : null,
            Category        = new List<string>(_category),
            Algorithms      = new List<string>(_algorithms),
            Olympiad        = _olympiad,
            Stage           = _stage,
            Year            = _year,
            Difficulty      = _difficulty,
            Source          = _source,
            SourceUrl       = _sourceUrl,
            SourceRaw       = _sourceRaw,
            ExplanationSource = _explanationSource,
            ContentCpp      = _contentCpp    != null ? new List<ContentBlock>(_contentCpp) : null,
            Options         = _options       != null ? new List<string>(_options) : null,
            MatchOptions    = _matchOptions  != null ? new List<string>(_matchOptions) : null,
            Explanation     = _explanation   != null ? new List<ContentBlock>(_explanation) : null,
            CorrectAnswer   = new List<string>(_correctAnswer),
            Points          = _points,
            PartialCredit   = _partialCredit
        };
    }
}
