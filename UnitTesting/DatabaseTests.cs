using System.Text.Json;

namespace UnitTesting;

public class DatabaseTests
{
    private const string TestFileName = "DatabaseTest.json";

    [TearDown]
    public void CleanUp()
    {
        File.Delete(TestFileName);
    }

    [Test]
    public void ReadWriteTest()
    {
        FileStream stream = new FileStream(TestFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        PlannedEvent plannedEvent = new PlannedEvent()
        {
            Name = "TestName",
            Place = "TestPlace",
            Start = DateTime.Now,
            Duration = 1,
        };

        stream.SetLength(0);

        EventsDatabaseReadWriter<PlannedEvent> database = new EventsDatabaseReadWriter<PlannedEvent>(stream);

        Assert.That(database.Events, Is.EqualTo(new List<PlannedEvent>()));

        database.Events.Add(plannedEvent);
        database.Dispose();
        stream.Close();

        database = new EventsDatabaseReadWriter<PlannedEvent>(TestFileName);
        Assert.That(database.Events, Is.EqualTo(new List<PlannedEvent>{plannedEvent}));
        database.Dispose();
    }
}