using UnityEngine;
using Verse;

namespace AutoNameBabies;

public class AutoNameBabiesMod : Mod
{
    public static AutoNameBabiesSettings Settings = null!;

    public AutoNameBabiesMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<AutoNameBabiesSettings>();
    }

    public override string SettingsCategory()
    {
        return "ANB.SettingsCategory".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoWindowContents(inRect);
    }
}

public enum SurnameMode
{
    Father,
    Mother,
    Random,
    Ideology
}

public class AutoNameBabiesSettings : ModSettings
{
    public SurnameMode surnameMode = SurnameMode.Random;

    public SurnameMode EffectiveSurnameMode
    {
        get
        {
            if (surnameMode == SurnameMode.Ideology && !ModsConfig.IdeologyActive)
            {
                return SurnameMode.Random;
            }

            return surnameMode;
        }
    }

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard list = new Listing_Standard();
        list.Begin(inRect);
        list.Label("ANB.SurnameHeader".Translate());
        list.Gap(6f);

        SurnameMode selected = EffectiveSurnameMode;

        if (list.RadioButton("ANB.SurnameFather".Translate(), selected == SurnameMode.Father, tooltip: "ANB.SurnameFatherTip".Translate()))
        {
            surnameMode = SurnameMode.Father;
        }

        if (list.RadioButton("ANB.SurnameMother".Translate(), selected == SurnameMode.Mother, tooltip: "ANB.SurnameMotherTip".Translate()))
        {
            surnameMode = SurnameMode.Mother;
        }

        if (list.RadioButton("ANB.SurnameRandom".Translate(), selected == SurnameMode.Random, tooltip: "ANB.SurnameRandomTip".Translate()))
        {
            surnameMode = SurnameMode.Random;
        }

        if (ModsConfig.IdeologyActive)
        {
            if (list.RadioButton("ANB.SurnameIdeology".Translate(), surnameMode == SurnameMode.Ideology, tooltip: "ANB.SurnameIdeologyTip".Translate()))
            {
                surnameMode = SurnameMode.Ideology;
            }
        }

        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref surnameMode, "surnameMode", SurnameMode.Random);
    }
}
