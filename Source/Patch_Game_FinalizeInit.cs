using HarmonyLib;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
public static class Patch_Game_FinalizeInit
{
    public static void Postfix()
    {
        // Runs inside new-game and load setup, where an exception aborts the map
        try
        {
            BabyNamer.NameUnnamedPlayerBabies();
        }
        catch (System.Exception exception)
        {
            Log.ErrorOnce($"[Auto Name Babies] Could not name existing babies; they keep their temporary names.\n{exception}", 173846922);
        }
    }
}
