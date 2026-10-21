using HireFlow.Application.Services;

namespace HireFlow.Tests;

// Tests for the rule-based CV match score.
public class MatchScoreCalculatorTests
{
    private readonly MatchScoreCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenAllSkillsFound_Returns100()
    {
        var score = _calculator.Calculate("C#, SQL Server", "I know c# and SQL Server very well.");

        Assert.Equal(100, score);
    }

    [Fact]
    public void Calculate_WhenNoSkillsFound_ReturnsZero()
    {
        var score = _calculator.Calculate("C#, SQL Server", "I am a great cook.");

        Assert.Equal(0, score);
    }

    [Fact]
    public void Calculate_WhenHalfOfSkillsFound_Returns50()
    {
        var score = _calculator.Calculate("Angular, Docker", "Five years of Angular.");

        Assert.Equal(50, score);
    }

    [Fact]
    public void Calculate_MatchesWholeWordsOnly()
    {
        // "Java" must not match inside "JavaScript".
        var score = _calculator.Calculate("Java", "I write JavaScript every day.");

        Assert.Equal(0, score);
    }

    [Fact]
    public void Calculate_WhenCvIsEmpty_ReturnsZero()
    {
        Assert.Equal(0, _calculator.Calculate("C#", ""));
    }
}
