using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;
using Verse.Sound;
using static HarmonyLib.AccessTools;

namespace Tale_for_Art
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            var harmony = new Harmony("localghost.taleforart");
            harmony.Patch(Method("Verse.ThingComp:CompGetGizmosExtra"), postfix: new HarmonyMethod(Method("Tale_for_Art.HarmonyPatches:Postfix")));
            harmony.Patch(Method(
#if v1_4
                    "RimWorld.TaleTextGenerator:GenerateTextFromTale"
#else
                    "RimWorld.TaleTextGenerator:GenerateTextFromTale",
                    new System.Type[]
                    {
                        typeof(TextGenerationPurpose),
                        typeof(Tale),
                        typeof(int),
                        typeof(List<RulePackDef>),
                        typeof(List<Rule>),
                        typeof(Dictionary<string, string>)
                    }
#endif
                ), transpiler: new HarmonyMethod(Method("Tale_for_Art.HarmonyPatches:Transpiler")));
            harmony.Patch(Method("RimWorld.MinifiedThing:Destroy"), prefix: new HarmonyMethod(Method("Tale_for_Art.HarmonyPatches:DestroyPostfix")));
        }
    }

    public static class HarmonyPatches
    {
        static void Postfix(ThingComp __instance, ref IEnumerable<Gizmo> __result)
        {
            var compArt = __instance as CompArt;
            if (compArt == null || !compArt.CanShowArt)
                return;
            var command_art = new Command_Art
            {
                defaultLabel = "Tale_for_Art.Randomize".Translate(),
                hotKey = KeyBindingDefOf.Misc10,
                defaultDesc = "Tale_for_Art.RandomizeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Structures/Random")
            };
            command_art.action = () => command_art.Randomize(compArt);
            var command_title = new Command_Title
            {
                defaultLabel = "Tale_for_Art.RenameTitle".Translate(),
                hotKey = KeyBindingDefOf.Misc11,
                defaultDesc = "Tale_for_Art.RenameTitleDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename")
            };
            command_title.action = () => command_title.Rename(compArt);
            __result = __result.Concat(command_art).Concat(command_title);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, typeof(Tale_for_Art_Mod).GetField(nameof(Tale_for_Art_Mod.settings)));
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(Settings).GetField(nameof(Settings.TalelessChanceWithTales)));
                }
                else
                    yield return instruction;
            }
        }
        static void DestroyPostfix(MinifiedThing __instance, DestroyMode mode)
        {
            if (__instance.InnerThing != null && !(__instance.InnerThing is MonumentMarker))
                __instance.InnerThing.Destroy(mode);
        }
    }
}
