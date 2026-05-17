using System.Text.Json;

namespace FolderHeat.App;

internal sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string path;

    public AppSettingsStore(string path)
    {
        this.path = path;
    }

    public HotkeySettings LoadHotkey()
    {
        if (!File.Exists(path))
        {
            return HotkeySettings.Default;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<HotkeySettings>(json) ?? HotkeySettings.Default;
        }
        catch (JsonException)
        {
            return HotkeySettings.Default;
        }
        catch (IOException)
        {
            return HotkeySettings.Default;
        }
    }

    public void SaveHotkey(HotkeySettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? AppContext.BaseDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(path, json);
    }
}
