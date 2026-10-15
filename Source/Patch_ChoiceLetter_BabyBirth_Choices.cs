using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.Choices), MethodType.Getter)]
public static class Patch_ChoiceLetter_BabyBirth_Choices
{
    private static readonly AccessTools.FieldRef<ChoiceLetter_BabyBirth, Pawn> PawnField =
        AccessTools.FieldRefAccess<ChoiceLetter_BabyBirth, Pawn>("pawn");

    private static readonly System.Reflection.MethodInfo JumpToLocationGetter =
        AccessTools.PropertyGetter(typeof(ChoiceLetter), "Option_JumpToLocation");

    private static readonly System.Reflection.MethodInfo CloseGetter =
        AccessTools.PropertyGetter(typeof(ChoiceLetter), "Option_Close");

    private static readonly AccessTools.FieldRef<DiaOption, string> OptionText =
        AccessTools.FieldRefAccess<DiaOption, string>("text");

    public static Pawn GetPawn(ChoiceLetter_BabyBirth letter)
    {
        return PawnField(letter);
    }

    public static void Postfix(ChoiceLetter_BabyBirth __instance, ref IEnumerable<DiaOption> __result)
    {
        Pawn? pawn = GetPawn(__instance);
        if (pawn?.Faction.IsPlayerSafe() == true && !BabyNamer.IsTemporarilyNamed(pawn))
        {
            __result = ChoicesAfterAutomaticNaming(__instance, __result);
        }
    }

    private static IEnumerable<DiaOption> ChoicesAfterAutomaticNaming(
        ChoiceLetter_BabyBirth letter,
        IEnumerable<DiaOption> original)
    {
        string nameBaby = "NameBaby".Translate().CapitalizeFirst();
        string postpone = "PostponeLetter".Translate();
        string jump = "JumpToLocation".Translate();
        string close = "Close".Translate();
        bool hasClose = false;

        foreach (DiaOption option in original)
        {
            string text = OptionText(option);
            if (text == nameBaby || text == postpone)
            {
                continue;
            }

            if (text == jump)
            {
                // Vanilla's active-letter jump option postpones the letter. Substitute
                // the closing version now that there is no naming decision outstanding.
                yield return (DiaOption)JumpToLocationGetter.Invoke(letter, null)!;
                continue;
            }

            if (text == close)
            {
                hasClose = true;
            }

            // Preserve choices contributed by other mods.
            yield return option;
        }

        if (!hasClose)
        {
            yield return (DiaOption)CloseGetter.Invoke(letter, null)!;
        }
    }
}
