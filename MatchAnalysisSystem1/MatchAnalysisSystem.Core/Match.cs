using System;
using System.Collections.Generic;
using System.Text;

namespace MatchAnalysisSystem.Core;

public class Match
{
    public int Id { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string League { get; set; } = string.Empty;
}