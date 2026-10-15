using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.Start))]
public static class Patch_ChoiceLetter_BabyBirth_Start
{
    public static void Postfix(ChoiceLetter_BabyBirth __instance)
    {
        Pawn? pawn = Patch_ChoiceLetter_BabyBirth_Choices.GetPawn(__instance);
        if (pawn == null)
        {
            return;
        }

        if (!pawn.Faction.IsPlayerSafe())
        {
            return;
        }

        Name previousName = pawn.Name;
        if (!BabyNamer.IsTemporarilyNamed(pawn) || BabyNamer.TryName(pawn))
        {
            UpdateLetterText(__instance, pawn, previousName);
        }
    }

    private static void UpdateLetterText(ChoiceLetter_BabyBirth letter, Pawn pawn, Name previousName)
    {
        string current = letter.Text.Resolve();
        string named = "ANB.LetterPartAutoNamed".Translate(pawn.Name.ToStringFull).Resolve();
        Name currentName = pawn.Name;
        string tempLive;
        string tempStill;
        string adopt;
        try
        {
            // The letter was composed before Start named the pawn, so resolve the text
            // against the former name in order to replace the exact original paragraph.
            pawn.Name = previousName;
            tempLive = ("LetterPartTempBabyName".Translate(pawn) + " " + "LetterPartLiveBirthNameDeadline".Translate(60000.ToStringTicksToPeriod())).Resolve();
            tempStill = ("LetterPartTempBabyName".Translate(pawn) + " " + "LetterPartStillbirthNameDeadline".Translate()).Resolve();
            adopt = "LetterPartNameBabyAdopt".Translate(pawn, 60000.ToStringTicksToPeriod()).Resolve();
        }
        finally
        {
            pawn.Name = currentName;
        }

        string updated = current
            .Replace("\n\n" + tempLive, "\n\n" + named)
            .Replace("\n\n" + tempStill, "\n\n" + named)
            .Replace("\n\n" + adopt, "\n\n" + named);

        if (updated != current)
        {
            letter.Text = updated;
        }
    }
}
