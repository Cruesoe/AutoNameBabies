using HarmonyLib;
using Verse;

namespace AutoNameBabies;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        new Harmony("cruesoe.autonamebabies").PatchAll();
    }
}
