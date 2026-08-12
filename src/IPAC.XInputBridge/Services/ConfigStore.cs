using System.Text.Json;
using System.Text.Json.Serialization;
using IPAC.XInputBridge.Models;

namespace IPAC.XInputBridge.Services;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IPAC XInput Bridge");

    public string FilePath => Path.Combine(DirectoryPath, "config.json");

    public AppConfig Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return AppConfig.CreateDefault();
            }

            AppConfig? config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(FilePath), JsonOptions);
            if (config is null || config.Profiles.Count == 0)
            {
                return AppConfig.CreateDefault();
            }

            foreach (ProfileConfig profile in config.Profiles)
            {
                while (profile.Players.Count < 4)
                {
                    profile.Players.Add(PlayerConfig.CreateDefault(profile.Players.Count));
                }
            }

            return config;
        }
        catch (JsonException)
        {
            string backup = FilePath + ".broken-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            File.Move(FilePath, backup, overwrite: true);
            return AppConfig.CreateDefault();
        }
    }

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(DirectoryPath);
        string temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(config, JsonOptions));
        File.Move(temporary, FilePath, overwrite: true);
    }
}
