using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CampfireStories
{
    public class WaitByFire : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var job = JobMaker.MakeJob(JobDefOf.Wait);
            var story = pawn.GetLord()?.LordJob as StoryLord;
            if (story != null && story.CurrentSpeaker == pawn) job.overrideFacing = story.AudienceFacing();
            job.expiryInterval = 120;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
    public class CampfireTarget : RitualObligationTargetWorker_AnyRitualSpot
    {
        public CampfireTarget() { }
        public CampfireTarget(RitualObligationTargetFilterDef def) : base(def) { }
        public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
        { foreach (var thing in map.listerThings.ThingsOfDef(ThingDefOf.Campfire)) yield return thing; }
        protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
        { return target.HasThing && target.Thing.def == ThingDefOf.Campfire && target.Thing.Spawned; }
        public override IEnumerable<string> GetTargetInfos(RitualObligation obligation) { yield return ThingDefOf.Campfire.label; }
    }

    public class StoryBehavior : RitualBehaviorWorker
    {
        public StoryBehavior() { }
        public StoryBehavior(RitualBehaviorDef def) : base(def) { }
        public override string CanStartRitualNow(TargetInfo target, Precept_Ritual ritual, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
        {
            if (!target.HasThing || target.Thing.def != ThingDefOf.Campfire || target.Thing.TryGetComp<CompGlower>()?.Glows != true)
                return "CS_FireRequired".Translate();
            return base.CanStartRitualNow(target, ritual, selectedPawn, forcedForRole);
        }
        protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
        { return new StoryLord(target, ritual, obligation, def.stages, assignments); }
    }

    public class StoryEnd : StageEndTrigger
    {
        public override bool CountsTowardsProgress => true;
        public override Trigger MakeTrigger(LordJob_Ritual ritual, TargetInfo spot, IEnumerable<TargetInfo> foci, RitualStage stage)
        { return new Trigger_TickCondition(() => ((StoryLord)ritual).Complete); }
    }

    public class StoryToil : LordToil_Ritual
    {
        public StoryToil(IntVec3 spot, StoryLord ritual, RitualStage stage) : base(spot, ritual, stage, null) { }
        public override void UpdateAllDuties()
        {
            base.UpdateAllDuties();
            var story = (StoryLord)ritual;
            var speaker = story.CurrentSpeaker;
            if (speaker == null || !lord.ownedPawns.Contains(speaker)) return;
            var fire = story.selectedTarget.Cell;
            var audience = story.AudienceCenter();
            var ideal = fire + (fire - audience).ToVector3().normalized.ToIntVec3();
            IntVec3 cell = GenRadial.RadialCellsAround(fire, 2, false)
                .Where(c => c != fire).OrderBy(c => c.DistanceToSquared(ideal))
                .FirstOrDefault(c => c.InBounds(speaker.Map) && c.Standable(speaker.Map) && !c.GetThingList(speaker.Map).Any(t => t is Pawn && t != speaker)
                    && speaker.CanReach(c, PathEndMode.OnCell, Danger.None));
            if (cell == default(IntVec3)) return;
            speaker.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("CS_TellStory"), cell, story.selectedTarget.Thing);
        }
    }
}
