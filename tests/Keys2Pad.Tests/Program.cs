using Keys2Pad.Models;

AppConfig config = AppConfig.CreateDefault();
Assert(config.Profiles.Count == 1, "Ein Default-Profil");
Assert(config.CurrentProfile.Players.Count == 4, "Vier Spieler");
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.A] == (int)Keys.LControlKey, "P1 A");
Assert(config.CurrentProfile.Players[1].Bindings[VirtualInput.Start] == (int)Keys.D6, "P2 Start");
Assert(config.CurrentProfile.Players[2].Bindings.Count == 0, "P3 absichtlich leer");
config.CurrentProfile.Players[0].Bindings[VirtualInput.Guide] = (int)Keys.Escape;
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.Guide] == (int)Keys.Escape, "Guide und Escape frei belegbar");

ProfileConfig copy = config.CurrentProfile.Clone("Kopie");
copy.Players[0].Bindings[VirtualInput.A] = (int)Keys.K;
Assert(config.CurrentProfile.Players[0].Bindings[VirtualInput.A] != copy.Players[0].Bindings[VirtualInput.A], "Deep clone");

Console.WriteLine("Alle Modell-Selbsttests bestanden.");

static void Assert(bool condition, string name)
{
    if (!condition)
    {
        throw new InvalidOperationException("Test fehlgeschlagen: " + name);
    }
}
