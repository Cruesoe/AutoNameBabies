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
