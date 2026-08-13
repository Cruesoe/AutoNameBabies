using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AutoNameBabies;

public static class BabyNamer
{
    public static string TemporaryBabyFirstName => "Baby".Translate().CapitalizeFirst();

    public static bool IsTemporarilyNamed(Pawn pawn)
    {
        return pawn?.Name is NameTriple triple && triple.First == TemporaryBabyFirstName;
    }

    public static bool TryName(Pawn pawn)
    {
        if (pawn == null || !ModsConfig.BiotechActive || !pawn.RaceProps.Humanlike)
        {
            return false;
        }

        if (!IsTemporarilyNamed(pawn))
        {
            return false;
        }

        if (pawn.Name is not NameTriple current)
        {
            return false;
        }

        string lastName = ChooseLastName(pawn) ?? current.Last;
        Name generated = PawnBioAndNameGenerator.GeneratePawnName(
            pawn,
            NameStyle.Full,
            lastName,
            forceNoNick: false,
            pawn.genes?.Xenotype);

        if (generated is NameTriple generatedTriple)
        {
            pawn.Name = new NameTriple(generatedTriple.First, generatedTriple.NickSet ? generatedTriple.Nick : null, lastName);
        }
        else if (generated is NameSingle generatedSingle)
        {
            pawn.Name = new NameTriple(generatedSingle.Name, null, lastName);
        }
        else
        {
            return false;
        }

        return true;
    }

    public static void NameUnnamedPlayerBabies()
    {
        if (!ModsConfig.BiotechActive || Find.World == null)
        {
            return;
        }

        foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
        {
            if (pawn.Faction.IsPlayerSafe() && IsTemporarilyNamed(pawn))
            {
                TryName(pawn);
            }
        }
    }

    public static string? ChooseLastName(Pawn baby)
    {
        return ChooseLastName(baby.GetMother(), baby.GetBirthParent(), baby.GetFather());
    }

    public static string? ChooseLastName(Pawn? geneticMother, Pawn? birthingMother, Pawn? father)
    {
        string? motherLast = LastNameOf(geneticMother);
        string? fatherLast = LastNameOf(father);
        string? birthLast = LastNameOf(birthingMother);

        switch (AutoNameBabiesMod.Settings.surnameMode)
        {
            case SurnameMode.Father:
                return FirstNonEmpty(fatherLast, motherLast, birthLast);
            case SurnameMode.Mother:
                return FirstNonEmpty(motherLast, birthLast, fatherLast);
            default:
                List<string> names = new List<string>();
                AddUnique(names, motherLast);
                AddUnique(names, fatherLast);
                if (birthingMother != null && birthingMother != geneticMother && birthingMother != father)
                {
                    AddUnique(names, birthLast);
                }

                if (names.Count == 0)
                {
                    return null;
                }

                return names.RandomElement();
        }
    }

    private static string? LastNameOf(Pawn? pawn)
    {
        if (pawn?.Name == null)
        {
            return null;
        }

        try
        {
            string last = PawnNamingUtility.GetLastName(pawn);
            return string.IsNullOrEmpty(last) ? null : last;
        }
        catch
        {
            return null;
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!value.NullOrEmpty())
            {
                return value;
            }
        }

        return null;
    }

    private static void AddUnique(List<string> names, string? name)
    {
        if (!name.NullOrEmpty() && !names.Contains(name!))
        {
            names.Add(name!);
        }
    }
}
