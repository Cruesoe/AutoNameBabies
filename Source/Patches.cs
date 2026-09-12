using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AutoNameBabies;

[HarmonyPatch(typeof(PregnancyUtility), "RandomLastName")]
public static class Patch_PregnancyUtility_RandomLastName
{
    public static bool Prefix(Pawn geneticMother, Pawn birthingMother, Pawn father, ref string __result)
    {
        if (BabyNamer.EffectiveSurnameMode == SurnameMode.Random)
        {
            return true;
        }

        try
        {
            string? lastName = BabyNamer.ChooseLastName(geneticMother, birthingMother, father);
            if (lastName.NullOrEmpty())
            {
                return true;
            }

            __result = lastName!;
            return false;
        }
        catch (System.Exception exception)
        {
            // A naming preference must never interrupt birth or pawn generation.
            // Let RimWorld choose the surname if another mod supplies unusual parents
            // or a partially initialized ideology.
            Log.ErrorOnce(
                $"[Auto Name Babies] Could not choose a custom baby surname; using RimWorld's default surname instead.\n{exception}",
                173846921);
            return true;
        }
    }
}

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
                yield return (DiaOption)JumpToLocationGetter.Invoke(letter, null);
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
            yield return (DiaOption)CloseGetter.Invoke(letter, null);
        }
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
