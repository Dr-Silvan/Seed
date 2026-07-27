namespace Seed.Models;

public sealed class AppState
{
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public string HabitName { get; set; } = "나의 목표";
    public List<AttemptRecord> Attempts { get; set; } = [];
    public List<TemptationRecord> Temptations { get; set; } = [];
    public bool WidgetEnabled { get; set; } = true;
    public bool BackgroundModeConfigured { get; set; }
}

public sealed class AttemptRecord
{
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public string Reason { get; set; } = "";
    public string Note { get; set; } = "";
}

public sealed class TemptationRecord
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Trigger { get; set; } = "";
    public bool Overcame { get; set; }
}

public sealed record GrowthStage(int Level, int MinimumDays, string Name, string Message);
