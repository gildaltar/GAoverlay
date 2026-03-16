using System.Text.Json;

namespace GAoverlay.Host.Services;

public sealed class LiveStateService
{
    private readonly string _liveStatePath;
    private readonly FileSystemWatcher _watcher;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public IReadOnlyDictionary<string, object?> CurrentState { get; private set; } = new Dictionary<string, object?>();
    public event Action? StateChanged;

    public LiveStateService(string liveStatePath)
    {
        _liveStatePath = liveStatePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_liveStatePath)!);

        if (!File.Exists(_liveStatePath))
        {
            File.WriteAllText(_liveStatePath, "{}
");
        }

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_liveStatePath)!, Path.GetFileName(_liveStatePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += (_, _) => Reload();
        _watcher.Created += (_, _) => Reload();
        _watcher.Renamed += (_, _) => Reload();

        Reload();
    }

    public void Publish(string key, object? value)
    {
        var clone = CurrentState.ToDictionary(k => k.Key, v => v.Value);
        clone[key] = value;
        Write(clone);
    }

    public void Publish(IReadOnlyDictionary<string, object?> values)
    {
        var clone = CurrentState.ToDictionary(k => k.Key, v => v.Value);
        foreach (var pair in values)
        {
            clone[pair.Key] = pair.Value;
        }
        Write(clone);
    }

    private void Write(Dictionary<string, object?> values)
    {
        File.WriteAllText(_liveStatePath, JsonSerializer.Serialize(values, _jsonOptions));
        CurrentState = values;
        StateChanged?.Invoke();
    }

    private void Reload()
    {
        try
        {
            using var stream = File.Open(_liveStatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var doc = JsonDocument.Parse(stream);
            var dict = new Dictionary<string, object?>();

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                dict[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Array => prop.Value.ToString(),
                    JsonValueKind.Object => prop.Value.ToString(),
                    _ => null
                };
            }

            CurrentState = dict;
            StateChanged?.Invoke();
        }
        catch
        {
            // Keep the last known good state. Computers are dramatic enough already.
        }
    }
}
