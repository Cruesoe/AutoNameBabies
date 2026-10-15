using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.ShouldAutomaticallyOpenLetter), MethodType.Getter)]
public static class Patch_ChoiceLetter_BabyBirth_ShouldAutomaticallyOpenLetter
{
    public static void Postfix(ChoiceLetter_BabyBirth __instance, ref bool __result)
    {
        Pawn? pawn = Patch_ChoiceLetter_BabyBirth_Choices.GetPawn(__instance);
        if (pawn?.Faction.IsPlayerSafe() == true && !BabyNamer.IsTemporarilyNamed(pawn))
        {
            __result = false;
        }
    }
}
