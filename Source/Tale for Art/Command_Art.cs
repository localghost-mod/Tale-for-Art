using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace Tale_for_Art
{
    public class Command_Art : Command_Action
    {
        public Command_Art() { }

        public void Randomize(CompArt compArt)
        {
            compArt.TaleRef?.ReferenceDestroyed();

            var seedinfo = typeof(TaleReference).GetField("seed", BindingFlags.Instance | BindingFlags.NonPublic);
            var talerefinfo = typeof(CompArt).GetField("taleRef", BindingFlags.Instance | BindingFlags.NonPublic);
            var titleinfo = typeof(CompArt).GetField("titleInt", BindingFlags.Instance | BindingFlags.NonPublic);

            Tale taleForArt = tale;
            if (tale == null)
                Tales.TryRandomElementByWeight(tale => tale.InterestLevel, out taleForArt);

            var taleref = new TaleReference(taleForArt);
            if (seed != -1)
                seedinfo.SetValue(taleref, seed);

            talerefinfo.SetValue(compArt, taleref);
            titleinfo.SetValue(compArt, (TaggedString)GenText.CapitalizeAsTitle(taleref.GenerateText(TextGenerationPurpose.ArtName, compArt.Props.nameMaker)));

            taleForArt.Notify_NewlyUsed();
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                yield return new FloatMenuOption(
                    "Tale_for_Art.TaleDef".Translate(taleDef?.label ?? "Random".Translate()),
                    () =>
                        Find.WindowStack.Add(
                            new FloatMenu(
                                TaleDefs
                                    .Select(taleDef => new FloatMenuOption(taleDef.label, () => Command_Art.taleDef = taleDef))
                                    .Append(new FloatMenuOption("Random".Translate(), () => taleDef = null))
                                    .ToList()
                            )
                        )
                );
                yield return new FloatMenuOption(
                    "Tale_for_Art.Pawn".Translate(pawn?.Name.ToStringShort ?? "Random".Translate()),
                    () =>
                        Find.WindowStack.Add(
                            new FloatMenu(
                                Pawns
                                    .Select(pawn => new FloatMenuOption(pawn.Name.ToStringFull, () => Command_Art.pawn = pawn))
                                    .Append(new FloatMenuOption("Random".Translate(), () => pawn = null))
                                    .ToList()
                            )
                        )
                );
                yield return new FloatMenuOption(
                    "Tale_for_Art.Tale".Translate(tale?.ToString() ?? "Random".Translate()),
                    () =>
                        Find.WindowStack.Add(
                            new FloatMenu(
                                Tales
                                    .Select(tale => new FloatMenuOption(tale.ToString(), () => Command_Art.tale = tale))
                                    .Append(new FloatMenuOption("Random".Translate(), () => tale = null))
                                    .ToList()
                            )
                        )
                );
                yield return new FloatMenuOption(
                    "Tale_for_Art.Seed".Translate(seed != -1 ? seed.ToString() : "Random".Translate().ToString()),
                    () => Find.WindowStack.Add(new Dialog_Seed())
                );
            }
        }
        public static IEnumerable<TaleDef> TaleDefs =>
            Find.TaleManager.AllTalesListForReading.Where(tale => tale.def.usableForArt && (pawn == null || tale.Concerns(pawn))).Select(tale => tale.def).Distinct();
        public static IEnumerable<Pawn> Pawns =>
            Find.TaleManager.AllTalesListForReading.Where(tale => tale.def.usableForArt && (taleDef == null || tale.def == taleDef))
                .Select(tale => tale.GetPawns())
                .SelectMany(x => x)
                .Distinct();

        public static IEnumerable<Tale> Tales =>
            Find.TaleManager.AllTalesListForReading.Where(tale => tale.def.usableForArt && (taleDef == null || tale.def == taleDef) && (pawn == null || tale.Concerns(pawn)));

        public static TaleDef taleDef;
        public static Pawn pawn;
        public static Tale tale;
        public static int seed = -1;
    }

    public static class TaleExtension
    {
        public static IEnumerable<Pawn> GetPawns(this Tale tale) =>
            tale.GetType().GetFields().Where(info => (info.GetValue(tale) as TaleData_Pawn)?.pawn.Name != null).Select(info => (info.GetValue(tale) as TaleData_Pawn).pawn);
    }

    public class Dialog_Seed : Dialog_Rename
    {
        protected override void SetName(string name) => Command_Art.seed = Int32.Parse(name);

        public override string Label => "Tale_for_Art.Seed".Translate(Command_Art.seed != -1 ? Command_Art.seed.ToString() : "Random".Translate().ToString());

        protected override AcceptanceReport NameIsValid(string name)
        {
            if (Int32.TryParse(name, out int value))
            {
                if (value >= -1)
                    return true;
                else
                    return "Tale_for_Art.MessageMustBeGreaterThanOrEqualToMinusOne".Translate();
            }
            return "Tale_for_Art.MessageNotValidInteger".Translate();
        }
    }
}
