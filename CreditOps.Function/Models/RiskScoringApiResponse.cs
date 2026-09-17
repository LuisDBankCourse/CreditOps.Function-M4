namespace CreditOps.Function.Models;

public class RiskScoringApiResponse
(
    int Score,
    string RiskLevel,
    string Decision,
    string Reason,
    Dictionary<string, int> RuleBreakdown

);