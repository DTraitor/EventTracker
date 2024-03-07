using Database;

namespace BusinessLayer;

public class DatabaseInteraction : IDisposable
{
    public DatabaseInteraction(string file)
    {
        database = new(file);
    }

    public DatabaseInteraction(FileStream stream)
    {
        database = new(stream);
    }

    public void Dispose()
    {
        database.Dispose();
    }

    public PlannedEvent CreateEvent(string name, string place, DateTime start, uint duration)
    {
        PlannedEvent result = new()
        {
            Name = name,
            Place = place,
            Start = start,
            Duration = duration,
        };
        database.Events.Add(result);
        database.Save();
        return result;
    }

    public void RemoveEvent(PlannedEvent plannedEvent)
    {
        database.Events.Remove(plannedEvent);
        database.Save();
    }

    public void ChangeName(PlannedEvent planned, string newName)
    {
        planned.Name = newName;
        database.Save();
    }

    public void ChangePlace(PlannedEvent planned, string newPlace)
    {
        planned.Place = newPlace;
        database.Save();
    }

    public void ReSchedule(PlannedEvent planned, DateTime newDate)
    {
        planned.Start = newDate;
        database.Save();
    }

    public void ChangeDuration(PlannedEvent planned, uint duration)
    {
        planned.Duration = duration;
        database.Save();
    }

    public List<PlannedEvent> GetAllEvents()
    {
        return database.Events;
    }

    public List<PlannedEvent> FindClosestEvents(DateTime time, int count)
    {
        List<PlannedEvent> closestEvents = new List<PlannedEvent>();
        foreach (PlannedEvent plannedEvent in database.Events)
        {
            if (plannedEvent.Start > time)
            {
                closestEvents.Add(plannedEvent);
            }
        }
        closestEvents.Sort((a, b) => a.Start.CompareTo(b.Start));
        if (closestEvents.Count > count)
            closestEvents.RemoveRange(count, closestEvents.Count-count);
        return closestEvents;
    }

    public List<PlannedEvent> FindOldEvents(DateTime time)
    {
        List<PlannedEvent> oldEvents = new List<PlannedEvent>();
        foreach (PlannedEvent plannedEvent in database.Events)
        {
            if (plannedEvent.Start < time)
            {
                oldEvents.Add(plannedEvent);
            }
        }
        return oldEvents;
    }

    public List<List<PlannedEvent>> FindIntersectingEvents()
    {
        List<List<PlannedEvent>> intersectingEvents = new List<List<PlannedEvent>>();
        foreach (PlannedEvent plannedEvent in database.Events)
        {
            List<PlannedEvent> intersecting = new List<PlannedEvent>();
            intersecting.Add(plannedEvent);
            foreach (PlannedEvent otherEvent in database.Events)
            {
                if (!ReferenceEquals(plannedEvent, otherEvent) && plannedEvent.Start < otherEvent.Start + TimeSpan.FromMinutes(otherEvent.Duration) && plannedEvent.Start + TimeSpan.FromMinutes(plannedEvent.Duration) > otherEvent.Start)
                {
                    intersecting.Add(otherEvent);
                }
            }
            if (intersecting.Count > 1)
            {
                intersectingEvents.Add(intersecting);
            }
        }

        for(int i = 0; i < intersectingEvents.Count; i++)
        {
            PlannedEvent first = intersectingEvents[i][0];
            for (int j = i + 1; j < intersectingEvents.Count; j++)
            {
                intersectingEvents[j].Remove(first);
            }
        }

        intersectingEvents.RemoveAll(list => list.Count < 2);

        return intersectingEvents;
    }

    private readonly EventsDatabaseReadWriter<PlannedEvent> database;
}


