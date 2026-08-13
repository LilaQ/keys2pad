using System.Text.Json;
using System.Text.Json.Serialization;
using Keys2Pad.Models;

namespace Keys2Pad.Services;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Keys2Pad");

    public string FilePath => Path.Combine(DirectoryPath, "config.json");

    private static IEnumerable<string> LegacyFilePaths
    {
        get
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            yield return Path.Combine(localAppData, "XInput KeyBridge", "config.json");
            yield return Path.Combine(localAppData, "IPAC XInput Bridge", "config.json");
        }
    }

    public AppConfig Load()
    {
        try
        {
            string? legacyFilePath = LegacyFilePaths.FirstOrDefault(File.Exists);
            if (!File.Exists(FilePath) && legacyFilePath is not null)
            {
                Directory.CreateDirectory(DirectoryPath);
                File.Copy(legacyFilePath, FilePath);
            }

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
