using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace IHateTechprints
{
    public class IHateTechprintsMod : Mod
    {
        public IHateTechprintsMod(ModContentPack pack) : base(pack)
        {
			new Harmony("IHateTechprintsMod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.TechprintRequirementMet), MethodType.Getter)]
    public static class ResearchProjectDef_TechprintRequirementMet_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(ResearchManager), "ApplyTechprint")]
    public static class ResearchManager_ApplyTechprint_Patch
    {
        public static bool Prefix(ResearchManager __instance, ResearchProjectDef proj, Pawn applyingPawn)
        {
            ApplyTechprint(__instance, proj, applyingPawn);
            return false;
        }

        public static void ApplyTechprint(ResearchManager __instance, ResearchProjectDef proj, Pawn applyingPawn)
        {
            if (!ModLister.CheckRoyalty("Techprint"))
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("LetterTechprintAppliedPartIntro".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
            stringBuilder.AppendLine();
            __instance.AddTechprints(proj, 1);
            if (proj.IsFinished)
            {
                stringBuilder.AppendLine("LetterTechprintAppliedPartAlreadyResearched".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
                stringBuilder.AppendLine();
            }
            else if (!proj.IsFinished)
            {
                float num = proj.baseCost / (float)proj.techprintCount;
                stringBuilder.AppendLine("IHT.LetterTechprintApplied".Translate(num, NamedArgumentUtility.Named(proj, "PROJECT")));
                stringBuilder.AppendLine();
                if (!__instance.progress.TryGetValue(proj, out var value))
                {
                    __instance.progress.Add(proj, Mathf.Min(num, proj.baseCost));
                }
                else
                {
                    __instance.progress[proj] = Mathf.Min(value + num, proj.baseCost);
                }
            }
            if (applyingPawn != null)
            {
                stringBuilder.AppendLine("LetterTechprintAppliedPartExpAwarded".Translate(2000.ToString(), SkillDefOf.Intellectual.label, applyingPawn.Named("PAWN")));
                applyingPawn.skills.Learn(SkillDefOf.Intellectual, 2000f, direct: true);
            }
            if (stringBuilder.Length > 0)
            {
                Find.LetterStack.ReceiveLetter("LetterTechprintAppliedLabel".Translate(NamedArgumentUtility.Named(proj, "PROJECT")), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent);
            }
        }
    }
}
