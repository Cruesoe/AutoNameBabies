using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.Choices), MethodType.Getter)]
public static class Patch_ChoiceLetter_BabyBirth_Choices
{
    private static readonly Func<ChoiceLetter, DiaOption> JumpToLocationOption =
        AccessTools.MethodDelegate<Func<ChoiceLetter, DiaOption>>(AccessTools.PropertyGetter(typeof(ChoiceLetter), "Option_JumpToLocation"));

    private static readonly Func<ChoiceLetter, DiaOption> CloseOption =
        AccessTools.MethodDelegate<Func<ChoiceLetter, DiaOption>>(AccessTools.PropertyGetter(typeof(ChoiceLetter), "Option_Close"));

    private static readonly AccessTools.FieldRef<DiaOption, string> OptionText =
        AccessTools.FieldRefAccess<DiaOption, string>("text");

    public static void Postfix(ChoiceLetter_BabyBirth __instance, ref IEnumerable<DiaOption> __result)
    {
        Pawn? pawn = BabyNamer.GetLetterPawn(__instance);
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
                // Swaps vanilla's jump-and-postpone option for the plain jump option
                yield return JumpToLocationOption(letter);
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
            yield return CloseOption(letter);
        }
    }
}
