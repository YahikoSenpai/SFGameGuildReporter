namespace SFGameGuildReporter.Models;

public class RaidReport
{
    public string RaidName { get; set; }
    public List<PlayerEntry> SignedUp { get; set; } = new();
    public List<PlayerEntry> NotSignedUp { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}