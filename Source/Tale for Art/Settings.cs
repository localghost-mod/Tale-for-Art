using UnityEngine;
using Verse;

namespace Tale_for_Art
{
    public class Settings : ModSettings
    {
        public float TalelessChanceWithTales = 0.2f;

        public override void ExposeData() => Scribe_Values.Look(ref TalelessChanceWithTales, "TalelessChanceWithTales");
    }

    public class Tale_for_Art_Mod : Mod
    {
        public static Settings settings;

        public Tale_for_Art_Mod(ModContentPack content)
            : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.SliderLabeled(
                "Tale_for_Art.TalelessChanceWithTales".Translate(),
                ref settings.TalelessChanceWithTales,
                settings.TalelessChanceWithTales.ToStringPercent(),
                0.0f,
                1.0f
            );
            if (ls.ButtonText("ResetButton".Translate()))
                settings.TalelessChanceWithTales = 0.2f;
            ls.End();
        }

        public override string SettingsCategory() => "Tale_for_Art.TaleforArt".Translate();
    }

    public static class Listing_StandardExtension
    {
        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = GenUI.Rounded(GenUI.LeftPart(rect, 0.5f));
            Rect rect3 = GenUI.Rounded(GenUI.LeftPart(GenUI.Rounded(GenUI.RightPart(rect, 0.3f)), 0.67f));
            Rect rect4 = GenUI.Rounded(GenUI.RightPart(rect, 0.1f));
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);
            float num = Widgets.HorizontalSlider(rect3, val, min, max);
            val = num;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect4, string.Format(format, val));
            if (!GenText.NullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }
    }
}
