using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
public static class Patch_PregnancyUtility_ApplyBirthOutcome
{
    public static void Postfix(Thing __result)
    {
        Pawn? pawn = __result as Pawn ?? (__result as Corpse)?.InnerPawn;
        if (pawn?.Faction.IsPlayerSafe() == true)
        {
            BabyNamer.TryName(pawn);
        }
    }
}
