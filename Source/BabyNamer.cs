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
        // Birth already rolled a last name through PregnancyUtility.RandomLastName.
        // Stochastic modes must keep it so the baby is not renamed a second time.
        if (baby.Name is NameTriple current && !current.Last.NullOrEmpty() && IsStochasticSurnameMode())
        {
            return current.Last;
        }

        return ChooseLastName(baby.GetMother(), baby.GetBirthParent(), baby.GetFather());
    }

    public static string? ChooseLastName(Pawn? geneticMother, Pawn? birthingMother, Pawn? father)
    {
        string? motherLast = LastNameOf(geneticMother);
        string? fatherLast = LastNameOf(father);
        string? birthLast = LastNameOf(birthingMother);

        switch (AutoNameBabiesMod.Settings.EffectiveSurnameMode)
        {
            case SurnameMode.Father:
                return FirstNonEmpty(fatherLast, motherLast, birthLast);
            case SurnameMode.Mother:
                return FirstNonEmpty(motherLast, birthLast, fatherLast);
            case SurnameMode.Ideology:
                return ChooseLastNameFromMarriagePrecept(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
            default:
                return RandomParentLastName(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
        }
    }

    private static bool IsStochasticSurnameMode()
    {
        SurnameMode mode = AutoNameBabiesMod.Settings.EffectiveSurnameMode;
        return mode == SurnameMode.Random || mode == SurnameMode.Ideology;
    }

    private static string? ChooseLastNameFromMarriagePrecept(
        Pawn? geneticMother,
        Pawn? birthingMother,
        Pawn? father,
        string? motherLast,
        string? fatherLast,
        string? birthLast)
    {
        Pawn? ideoPawn = ChooseIdeoPawn(geneticMother, birthingMother, father);
        if (ideoPawn?.Ideo == null)
        {
            return RandomParentLastName(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
        }

        string? preceptDefName = GetMarriageNamePreceptDefName(ideoPawn.Ideo);
        switch (preceptDefName)
        {
            case "MarriageName_AlwaysMans":
                return LastNameForMarriageChange(MarriageNameChange.MansName, geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
            case "MarriageName_AlwaysWomans":
                return LastNameForMarriageChange(MarriageNameChange.WomansName, geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
            case "MarriageName_KeepNames":
                return RandomParentLastName(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
            default:
                MarriageNameChange change = SpouseRelationUtility.Roll_NameChangeOnMarriage(ideoPawn);
                return LastNameForMarriageChange(change, geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
        }
    }

    private static string? LastNameForMarriageChange(
        MarriageNameChange change,
        Pawn? geneticMother,
        Pawn? birthingMother,
        Pawn? father,
        string? motherLast,
        string? fatherLast,
        string? birthLast)
    {
        Pawn? man;
        Pawn? woman;
        if (TryGetManAndWomanParents(geneticMother, birthingMother, father, out man, out woman))
        {
            switch (change)
            {
                case MarriageNameChange.MansName:
                    return FirstNonEmpty(LastNameOf(man), LastNameOf(woman), birthLast);
                case MarriageNameChange.WomansName:
                    return FirstNonEmpty(LastNameOf(woman), LastNameOf(man), birthLast);
                default:
                    return RandomParentLastName(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
            }
        }

        switch (change)
        {
            case MarriageNameChange.MansName:
                return FirstNonEmpty(fatherLast, motherLast, birthLast);
            case MarriageNameChange.WomansName:
                return FirstNonEmpty(motherLast, birthLast, fatherLast);
            default:
                return RandomParentLastName(geneticMother, birthingMother, father, motherLast, fatherLast, birthLast);
        }
    }

    private static bool TryGetManAndWomanParents(Pawn? geneticMother, Pawn? birthingMother, Pawn? father, out Pawn? man, out Pawn? woman)
    {
        Pawn? first = geneticMother ?? birthingMother;
        Pawn? second = father;
        if (second == null || second == first)
        {
            second = birthingMother != first ? birthingMother : null;
        }

        if (first == null || second == null)
        {
            man = null;
            woman = null;
            return false;
        }

        SpouseRelationUtility.DetermineManAndWomanSpouses(first, second, out Pawn determinedMan, out Pawn determinedWoman);
        man = determinedMan;
        woman = determinedWoman;
        return true;
    }

    private static Pawn? ChooseIdeoPawn(Pawn? geneticMother, Pawn? birthingMother, Pawn? father)
    {
        Pawn? first = geneticMother ?? birthingMother;
        Pawn? second = father;
        if (first?.Ideo != null && second?.Ideo != null && first.Ideo != second.Ideo)
        {
            return Rand.Value < 0.5f ? first : second;
        }

        if (first?.Ideo != null)
        {
            return first;
        }

        if (second?.Ideo != null)
        {
            return second;
        }

        if (birthingMother?.Ideo != null)
        {
            return birthingMother;
        }

        return first ?? second ?? birthingMother;
    }

    private static string? GetMarriageNamePreceptDefName(Ideo ideo)
    {
        List<Precept> precepts = ideo.PreceptsListForReading;
        for (int i = 0; i < precepts.Count; i++)
        {
            if (precepts[i].def.issue?.defName == "MarriageName")
            {
                return precepts[i].def.defName;
            }
        }

        return null;
    }

    private static string? RandomParentLastName(
        Pawn? geneticMother,
        Pawn? birthingMother,
        Pawn? father,
        string? motherLast,
        string? fatherLast,
        string? birthLast)
    {
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
