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
    Random
}

public class AutoNameBabiesSettings : ModSettings
{
    public SurnameMode surnameMode = SurnameMode.Random;

    public void DoWindowContents(Rect inRect)
    {
        Listing_Standard list = new Listing_Standard();
        list.Begin(inRect);
        list.Label("ANB.SurnameHeader".Translate());
        list.Gap(6f);

        if (list.RadioButton("ANB.SurnameFather".Translate(), surnameMode == SurnameMode.Father, tooltip: "ANB.SurnameFatherTip".Translate()))
        {
            surnameMode = SurnameMode.Father;
        }

        if (list.RadioButton("ANB.SurnameMother".Translate(), surnameMode == SurnameMode.Mother, tooltip: "ANB.SurnameMotherTip".Translate()))
        {
            surnameMode = SurnameMode.Mother;
        }

        if (list.RadioButton("ANB.SurnameRandom".Translate(), surnameMode == SurnameMode.Random, tooltip: "ANB.SurnameRandomTip".Translate()))
        {
            surnameMode = SurnameMode.Random;
        }

        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref surnameMode, "surnameMode", SurnameMode.Random);
    }
}
