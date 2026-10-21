using System.Text.RegularExpressions;
using HireFlow.Application.Interfaces;

namespace HireFlow.Application.Services;

// Gives a CV a score from 0 to 100: the percentage of the job's required skills found in the CV.
// This is rule-based on purpose (simple, explainable, free). It sits behind an interface,
// so an AI model could replace it later without changing the rest of the app.
public class MatchScoreCalculator : IMatchScoreCalculator
{
    public int Calculate(string requiredSkills, string cvText)
    {
        var skills = requiredSkills
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (skills.Count == 0 || string.IsNullOrWhiteSpace(cvText))
        {
            return 0;
        }

        var found = 0;
        foreach (var skill in skills)
        {
            if (CvContainsSkill(cvText, skill))
            {
                found++;
            }
        }

        return found * 100 / skills.Count;
    }

    // "Whole word" match, so skill "C" does not match the word "cat". Works for "C#" and ".NET" too.
    private static bool CvContainsSkill(string cvText, string skill)
    {
        var pattern = @"(?<![A-Za-z0-9])" + Regex.Escape(skill) + @"(?![A-Za-z0-9])";
        return Regex.IsMatch(cvText, pattern, RegexOptions.IgnoreCase);
    }
}
