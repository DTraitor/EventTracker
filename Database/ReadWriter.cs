using System.Runtime.Serialization;
using System.Text.Json;

namespace Database;

public class EventsDatabaseReadWriter<T> : IDisposable{
    public EventsDatabaseReadWriter(string file)
    {
        stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        Read();
    }

    public EventsDatabaseReadWriter(FileStream stream)
    {
        streamOwned = false;
        this.stream = stream;
        Read();
    }

    public void Dispose()
    {
        Save();
        if (streamOwned)
            stream.Dispose();
    }

    public void Read()
    {
        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, leaveOpen: true);
        try
        {
            Events = JsonSerializer.Deserialize<List<T>>(reader.ReadToEnd()) ?? new List<T>();
        }
        catch (JsonException e)
        {
            Events = new List<T>();
        }
    }

    public void Save()
    {
        stream.SetLength(0);
        using StreamWriter writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonSerializer.Serialize(Events));
    }

    public List<T> Events { get; private set; }
    private readonly bool streamOwned = true;
    private readonly FileStream stream;
}