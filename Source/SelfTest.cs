using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CampfireStories
{
    // Opt-in developer check. Never runs during ordinary play and never sends API requests.
    [StaticConstructorOnStartup]
    public static class StorySelfTest
    {
        static StorySelfTest()
        {
            if (!Environment.GetCommandLineArgs().Contains("-campfire-selftest")) return;
            LongEventHandler.ExecuteWhenFinished(Run);
        }
        private static void Check(bool value, string name)
        { if (!value) throw new Exception(name); Log.Message("[CampfireSelfTest] PASS " + name); }
        private static void Run()
        {
            try
            {
                foreach (var error in LanguageDatabase.activeLanguage.loadErrors.Concat(LanguageDatabase.activeLanguage.defInjections.SelectMany(p => p.loadErrors)))
                    Log.Message("[CampfireSelfTest] Translation diagnostic: " + error);
                var precept = DefDatabase<PreceptDef>.GetNamed("CS_CampfireStories");
                Check(precept.ritualPatternBase != null, "precept pattern resolves");
                var pattern = precept.ritualPatternBase;
                Check(pattern.ritualBehavior.workerClass == typeof(StoryBehavior), "behavior resolves");
                Check(pattern.ritualObligationTargetFilter.GetInstance() is CampfireTarget, "campfire target constructs");
                Check(pattern.ritualOutcomeEffect != null, "outcome resolves");
                Check(pattern.ritualBehavior.stages[0].endTriggers[0] is StoryEnd, "completion trigger resolves");
                Check(pattern.ritualBehavior.roles[0].maxCount == 3, "three optional storytellers");
                Check(DefDatabase<DutyDef>.GetNamed("CS_TellStory").thinkNode != null, "speaker duty resolves");
                Check(DefDatabase<ThoughtDef>.GetNamed("CS_Warm").stages[0].baseMoodEffect == 5, "mood outcome resolves");
                Check("CS_Title".Translate().ToString() != "CS_Title", "localization resolves");


                Log.Message("[CampfireSelfTest] COMPLETE 9 checks; no API calls.");
            }
            catch (Exception ex) { Log.Error("[CampfireSelfTest] FAIL " + ex); }
        }
    }
}
