namespace HireFlow.Application.Interfaces;

public interface IMatchScoreCalculator
{
    // Returns a score from 0 to 100.
    int Calculate(string requiredSkills, string cvText);
}
