using Keys2Pad.Models;

AppConfig config = AppConfig.CreateDefault();
Assert(config.Profiles.Count == 1, "One default profile");
Assert(config.ActiveProfile == "Default", "English default profile name");
Assert(config.CurrentProfile.Players.Count == 4, "Four players");
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.A] == (int)Keys.LControlKey, "P1 A");
Assert(config.CurrentProfile.Players[1].Bindings[VirtualInput.Start] == (int)Keys.D6, "P2 Start");
Assert(config.CurrentProfile.Players[2].Bindings.Count == 0, "P3 intentionally empty");
config.CurrentProfile.Players[0].Bindings[VirtualInput.Guide] = (int)Keys.Escape;
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.Guide] == (int)Keys.Escape, "Guide and Escape are freely mappable");

ProfileConfig copy = config.CurrentProfile.Clone("Copy");
copy.Players[0].Bindings[VirtualInput.A] = (int)Keys.K;
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.A] != copy.Players[0].Bindings[VirtualInput.A], "Deep clone");

RuntimeLogTests.Run();
DeviceFilterTests.Run();
Console.WriteLine("All model and logging self-tests passed.");

static void Assert(bool condition, string name)
{
    if (!condition)
    {
        throw new InvalidOperationException("Test failed: " + name);
    }
}
