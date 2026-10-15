using UnityEngine;
using Verse;

namespace AutoNameBabies;

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

        // Shown greyed out without Ideology so the option stays discoverable
        bool ideologyActive = ModsConfig.IdeologyActive;
        string ideologyTip = ideologyActive ? "ANB.SurnameIdeologyTip".Translate() : "ANB.SurnameIdeologyRequiresTip".Translate();
        if (list.RadioButton("ANB.SurnameIdeology".Translate(), selected == SurnameMode.Ideology, 0f, ideologyTip, null, !ideologyActive) && ideologyActive)
        {
            surnameMode = SurnameMode.Ideology;
        }

        list.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref surnameMode, "surnameMode", SurnameMode.Random);
    }
}
