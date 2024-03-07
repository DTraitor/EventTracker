namespace BusinessLayer;

public class PlannedEvent
{
    public string Name { get; set; }
    public string Place { get; set; }
    // When the event starts
    public DateTime Start { get; set; }
    // Duration in minutes
    public uint Duration { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PlannedEvent pEvent &&
                Name == pEvent.Name &&
                Place == pEvent.Place &&
                Start.Equals(pEvent.Start) &&
                Duration == pEvent.Duration;
    }
}