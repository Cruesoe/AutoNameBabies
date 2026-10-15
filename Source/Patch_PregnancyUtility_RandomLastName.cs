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
