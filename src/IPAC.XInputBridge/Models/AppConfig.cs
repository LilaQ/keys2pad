using System.Text.Json.Serialization;

namespace IPAC.XInputBridge.Models;

public sealed class AppConfig
{
    public string ActiveProfile { get; set; } = "Arcade Standard";
    public bool StartEnabled { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public int PollIntervalMs { get; set; } = 8;
    public List<ProfileConfig> Profiles { get; set; } = [];

    [JsonIgnore]
    public ProfileConfig CurrentProfile =>
        Profiles.FirstOrDefault(p => p.Name.Equals(ActiveProfile, StringComparison.OrdinalIgnoreCase))
        ?? Profiles.First();

    public static AppConfig CreateDefault() => new()
    {
        Profiles = [ProfileConfig.CreateDefault("Arcade Standard")]
    };
}

public sealed class ProfileConfig
{
    public string Name { get; set; } = "Profil";
    public List<PlayerConfig> Players { get; set; } = [];

    public static ProfileConfig CreateDefault(string name) => new()
    {
        Name = name,
        Players = Enumerable.Range(0, 4).Select(PlayerConfig.CreateDefault).ToList()
    };

    public ProfileConfig Clone(string name) => new()
    {
        Name = name,
        Players = Players.Select(p => new PlayerConfig
        {
            Enabled = p.Enabled,
            Bindings = new Dictionary<VirtualInput, int>(p.Bindings)
        }).ToList()
    };
}

public sealed class PlayerConfig
{
    public bool Enabled { get; set; } = true;
    public Dictionary<VirtualInput, int> Bindings { get; set; } = [];

    public static PlayerConfig CreateDefault(int playerIndex)
    {
        // P1/P2 follow common I-PAC defaults; P3/P4 are intentionally unbound.
        Dictionary<VirtualInput, int> keys = playerIndex switch
        {
            0 => Bind(Keys.Up, Keys.Down, Keys.Left, Keys.Right,
                Keys.LControlKey, Keys.LMenu, Keys.Space, Keys.LShiftKey,
                Keys.Z, Keys.X, Keys.D1, Keys.D5),
            1 => Bind(Keys.R, Keys.F, Keys.D, Keys.G,
                Keys.A, Keys.S, Keys.Q, Keys.W,
                Keys.I, Keys.K, Keys.D2, Keys.D6),
            _ => []
        };

        return new PlayerConfig { Bindings = keys };
    }

    private static Dictionary<VirtualInput, int> Bind(
        Keys up, Keys down, Keys left, Keys right,
        Keys a, Keys b, Keys x, Keys y,
        Keys lb, Keys rb, Keys back, Keys start) => new()
    {
        [VirtualInput.LeftStickUp] = (int)up,
        [VirtualInput.LeftStickDown] = (int)down,
        [VirtualInput.LeftStickLeft] = (int)left,
        [VirtualInput.LeftStickRight] = (int)right,
        [VirtualInput.DPadUp] = (int)up,
        [VirtualInput.DPadDown] = (int)down,
        [VirtualInput.DPadLeft] = (int)left,
        [VirtualInput.DPadRight] = (int)right,
        [VirtualInput.A] = (int)a,
        [VirtualInput.B] = (int)b,
        [VirtualInput.X] = (int)x,
        [VirtualInput.Y] = (int)y,
        [VirtualInput.LeftShoulder] = (int)lb,
        [VirtualInput.RightShoulder] = (int)rb,
        [VirtualInput.Back] = (int)back,
        [VirtualInput.Start] = (int)start
    };
}

public enum VirtualInput
{
    LeftStickUp,
    LeftStickDown,
    LeftStickLeft,
    LeftStickRight,
    RightStickUp,
    RightStickDown,
    RightStickLeft,
    RightStickRight,
    DPadUp,
    DPadDown,
    DPadLeft,
    DPadRight,
    A,
    B,
    X,
    Y,
    LeftShoulder,
    RightShoulder,
    LeftTrigger,
    RightTrigger,
    Back,
    Start,
    LeftThumb,
    RightThumb
}
