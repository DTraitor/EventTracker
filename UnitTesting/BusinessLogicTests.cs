using System.Text.Json;

namespace UnitTesting;

public class BusinessLogicTests
{
    private const string TestFileName = "BusinessLogicTest.json";

    [TearDown]
    public void CleanUp()
    {
        File.Delete(TestFileName);
    }

    [Test]
    public void InteractionTest()
    {
        FileStream stream = new FileStream(TestFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        stream.SetLength(0);
        DatabaseInteraction interaction = new DatabaseInteraction(stream);
        PlannedEvent createdEvent = interaction.CreateEvent("Name", "Place", DateTime.Today, 20);

        interaction.ChangeName(createdEvent, "TestName");
        interaction.ChangePlace(createdEvent, "TestPlace");
        interaction.ReSchedule(createdEvent, DateTime.Now.AddDays(-1));
        interaction.ChangeDuration(createdEvent, 1);

        Assert.That(interaction.GetAllEvents(), Is.EqualTo(new List<PlannedEvent>{createdEvent}));
        stream.Position = 0;
        using (StreamReader reader = new StreamReader(stream, leaveOpen: true))
        {
            Assert.That(reader.ReadToEnd(), Is.EqualTo(JsonSerializer.Serialize(new List<PlannedEvent>{createdEvent})));
        }

        PlannedEvent second = interaction.CreateEvent("Name", "Place", DateTime.Now.AddDays(2), 20);
        PlannedEvent third = interaction.CreateEvent("Name", "Place", DateTime.Now.AddDays(1).AddHours(23), 70);
        PlannedEvent fourth = interaction.CreateEvent("Name", "Place", DateTime.Now.AddHours(1), 10);

        Assert.That(interaction.FindClosestEvents(DateTime.Now, 2), Is.EqualTo(new List<PlannedEvent>{fourth, third}));
        Assert.That(interaction.FindOldEvents(DateTime.Now), Is.EqualTo(new List<PlannedEvent>{createdEvent}));

        interaction.RemoveEvent(fourth);
        interaction.RemoveEvent(createdEvent);
        Assert.That(interaction.GetAllEvents(), Is.EqualTo(new List<PlannedEvent>{second, third}));

        Assert.That(interaction.FindIntersectingEvents(), Is.EqualTo(new List<List<PlannedEvent>>{new (){second, third}}));

        stream.Close();

        interaction = new DatabaseInteraction(TestFileName);
        Assert.That(interaction.GetAllEvents(), Is.EqualTo(new List<PlannedEvent>{second, third}));
        interaction.Dispose();
    }
}