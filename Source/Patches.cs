using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(PregnancyUtility), "RandomLastName")]
public static class Patch_PregnancyUtility_RandomLastName
{
    public static bool Prefix(Pawn geneticMother, Pawn birthingMother, Pawn father, ref string __result)
    {
        if (AutoNameBabiesMod.Settings.surnameMode == SurnameMode.Random)
        {
            return true;
        }

        string? lastName = BabyNamer.ChooseLastName(geneticMother, birthingMother, father);
        if (lastName.NullOrEmpty())
        {
            return true;
        }

        __result = lastName!;
        return false;
    }
}

[HarmonyPatch(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo))]
public static class Patch_GiveAppropriateBioAndNameTo
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.Faction.IsPlayerSafe())
        {
            BabyNamer.TryName(pawn);
        }
    }
}

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

        if (pawn.Faction.IsPlayerSafe())
        {
            BabyNamer.TryName(pawn);
        }

        UpdateLetterText(__instance, pawn);
    }

    private static void UpdateLetterText(ChoiceLetter_BabyBirth letter, Pawn pawn)
    {
        string current = letter.Text.Resolve();
        string named = "ANB.LetterPartAutoNamed".Translate(pawn.Name.ToStringFull).Resolve();
        string tempLive = ("LetterPartTempBabyName".Translate(pawn) + " " + "LetterPartLiveBirthNameDeadline".Translate(60000.ToStringTicksToPeriod())).Resolve();
        string tempStill = ("LetterPartTempBabyName".Translate(pawn) + " " + "LetterPartStillbirthNameDeadline".Translate()).Resolve();
        string adopt = "LetterPartNameBabyAdopt".Translate(pawn, 60000.ToStringTicksToPeriod()).Resolve();

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

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.ShouldAutomaticallyOpenLetter), MethodType.Getter)]
public static class Patch_ChoiceLetter_BabyBirth_ShouldAutomaticallyOpenLetter
{
    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}

[HarmonyPatch(typeof(ChoiceLetter_BabyBirth), nameof(ChoiceLetter_BabyBirth.Choices), MethodType.Getter)]
public static class Patch_ChoiceLetter_BabyBirth_Choices
{
    private static readonly AccessTools.FieldRef<ChoiceLetter_BabyBirth, Pawn> PawnField =
        AccessTools.FieldRefAccess<ChoiceLetter_BabyBirth, Pawn>("pawn");

    public static Pawn GetPawn(ChoiceLetter_BabyBirth letter)
    {
        return PawnField(letter);
    }

    private static readonly AccessTools.FieldRef<DiaOption, string> DiaOptionText =
        AccessTools.FieldRefAccess<DiaOption, string>("text");

    public static void Postfix(ref IEnumerable<DiaOption> __result)
    {
        string nameBaby = "NameBaby".Translate().CapitalizeFirst();
        __result = __result.Where(option => DiaOptionText(option) != nameBaby);
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
public static class Patch_Game_FinalizeInit
{
    public static void Postfix()
    {
        BabyNamer.NameUnnamedPlayerBabies();
    }
}
