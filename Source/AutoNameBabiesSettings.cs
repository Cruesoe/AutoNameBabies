using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AutoNameBabies;

public class AutoNameBabiesSettings : ModSettings
{
    public SurnameMode surnameMode = SurnameMode.Random;
    public string boyNames = "";
    public string girlNames = "";

    private Vector2 boyScroll;
    private Vector2 girlScroll;

    public static List<string> ParseNames(string? text)
    {
        List<string> names = new List<string>();
        if (text.NullOrEmpty())
        {
            return names;
        }

        foreach (string line in text!.Split('\n'))
        {
            string name = line.Trim();
            if (name.Length > 0 && !names.Contains(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

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

        list.GapLine(18f);
        list.Label("ANB.FirstNamesHeader".Translate());
        list.Label("ANB.FirstNamesDesc".Translate());
        list.Gap(6f);

        float labelHeight = Text.LineHeight;
        Rect area = list.GetRect(Mathf.Max(160f, inRect.height - list.CurHeight - 12f));
        float columnWidth = (area.width - 16f) / 2f;
        Rect boys = new Rect(area.x, area.y, columnWidth, area.height);
        Rect girls = new Rect(boys.xMax + 16f, area.y, columnWidth, area.height);

        Widgets.Label(new Rect(boys.x, boys.y, boys.width, labelHeight), "ANB.FirstNamesBoys".Translate());
        boyNames = ScrollingTextArea(new Rect(boys.x, boys.y + labelHeight + 4f, boys.width, boys.height - labelHeight - 4f), boyNames, ref boyScroll, BoyNamesControl);
        Widgets.Label(new Rect(girls.x, girls.y, girls.width, labelHeight), "ANB.FirstNamesGirls".Translate());
        girlNames = ScrollingTextArea(new Rect(girls.x, girls.y + labelHeight + 4f, girls.width, girls.height - labelHeight - 4f), girlNames, ref girlScroll, GirlNamesControl);

        list.End();

        // Enter adds a line instead of closing the settings window while a name list has focus
        string focused = GUI.GetNameOfFocusedControl();
        Window? window = Find.WindowStack.currentlyDrawnWindow;
        if (window != null)
        {
            window.closeOnAccept = focused != BoyNamesControl && focused != GirlNamesControl;
        }
    }

    private const string BoyNamesControl = "ANB_BoyNames";
    private const string GirlNamesControl = "ANB_GirlNames";

    private static string ScrollingTextArea(Rect outRect, string text, ref Vector2 scroll, string controlName)
    {
        float innerWidth = outRect.width - 16f;
        Rect inner = new Rect(0f, 0f, innerWidth, Mathf.Max(outRect.height, Text.CalcHeight(text, innerWidth) + 24f));
        Widgets.BeginScrollView(outRect, ref scroll, inner);
        GUI.SetNextControlName(controlName);
        string result = Widgets.TextArea(inner, text);
        Widgets.EndScrollView();
        return result;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref surnameMode, "surnameMode", SurnameMode.Random);
        Scribe_Values.Look(ref boyNames, "boyNames", "");
        Scribe_Values.Look(ref girlNames, "girlNames", "");
    }
}
