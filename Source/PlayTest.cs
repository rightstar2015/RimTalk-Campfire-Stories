using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI.Group;

namespace CampfireStories
{
    // Only an explicitly launched disposable quicktest can activate this integration check.
    public sealed class CampfirePlayTest : GameComponent
    {
        private readonly bool enabled;
        private bool started;
        private bool ended;
        private int startTick;
        private StoryLord story;
        private readonly HashSet<Pawn> seen = new HashSet<Pawn>(); private readonly HashSet<Pawn> faced = new HashSet<Pawn>();
        public CampfirePlayTest(Game game)
        {
            var args = Environment.GetCommandLineArgs();
            enabled = args.Contains("-campfire-playtest") && args.Contains("-quicktest");
        }
        public override void GameComponentUpdate()
        {
            if (!enabled || ended || Current.ProgramState != ProgramState.Playing || Find.CurrentMap == null) return;
            try
            {
                Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
                if (!started) Start();
                if (story.CurrentSpeaker != null)
                {
                    var speaker = story.CurrentSpeaker;
                    seen.Add(speaker);
                    if (speaker.CurJob?.def == JobDefOf.Wait && speaker.CurJob.overrideFacing != Rot4.Invalid
                        && speaker.Rotation == story.AudienceFacing()) faced.Add(speaker);
                }
                if (story.Complete && !Find.CurrentMap.lordManager.lords.Any(l => l.LordJob == story))
                {
                    if (faced.Count != 3) throw new Exception("Not every storyteller faced the audience: " + faced.Count);
                    if (seen.Count != 3) throw new Exception("Expected three distinct speakers; observed " + seen.Count);
                    if (story.PlayedLineCount != 0) throw new Exception("Expected no generated lines; observed " + story.PlayedLineCount);
                    if (!seen.Any(p => p.needs.mood.thoughts.memories.Memories.Any(m => m.def.defName.StartsWith("CS_"))))
                        throw new Exception("No ritual mood outcome was applied.");
                    Log.Message("[CampfirePlayTest] PASS ordinary ritual mood applied without generated dialogue.");
                    Log.Message("[CampfirePlayTest] PASS three speakers completed the no-API ritual and faced the audience.");
                    ended = true;
                }
                if (GenTicks.TicksGame - startTick > 10000) throw new Exception("Ritual did not complete within test budget.");
            }
            catch (Exception ex) { ended = true; Log.Error("[CampfirePlayTest] FAIL " + ex); }
        }
        private void Start()
        {
            var map = Find.CurrentMap;
            var pawns = map.mapPawns.FreeColonistsSpawned.Take(3).ToList();
            if (pawns.Count < 3) throw new Exception("Quicktest needs three colonists.");
            var spot = GenRadial.RadialCellsAround(pawns[0].Position, 12, true)
                .First(c => c.InBounds(map) && c.Standable(map) && c.GetEdifice(map) == null && !c.GetThingList(map).Any(t => t is Pawn));
            var fire = ThingMaker.MakeThing(ThingDefOf.Campfire);
            fire.SetFaction(Faction.OfPlayer);
            GenSpawn.Spawn(fire, spot, map);
            fire.TryGetComp<CompRefuelable>().Refuel(20);
            var preceptDef = DefDatabase<PreceptDef>.GetNamed("CS_CampfireStories");
            var ritual = (Precept_Ritual)PreceptMaker.MakePrecept(preceptDef);
            pawns[0].Ideo.AddPrecept(ritual, true, null, preceptDef.ritualPatternBase);
            if (ModsConfig.IsActive("cj.rimtalk"))
            {
                if (StoryIntegration.Service == null) throw new Exception("Optional adapter failed to load.");
                StoryIntegration.Service.GetType().GetMethod("TestMemories").Invoke(StoryIntegration.Service, new object[] { pawns[0], pawns[1] });
            }
            else if (StoryIntegration.Service != null) throw new Exception("Unexpected integration in core-only test.");
            var assignments = new RitualRoleAssignments(ritual, fire);
            assignments.Setup(pawns, new List<Pawn>());
            foreach (var pawn in pawns) assignments.TryAssign(pawn, ritual.behavior.def.roles[0], out var reason);

            if (StoryIntegration.Ready) throw new Exception("Refusing API-enabled test environment.");
            ritual.behavior.TryExecuteOn(fire, pawns[0], ritual, null, assignments, true);
            story = map.lordManager.lords.Select(l => l.LordJob).OfType<StoryLord>().FirstOrDefault();
            if (story == null) throw new Exception("Ritual start failed: " + ritual.behavior.CanStartRitualNow(fire, ritual));
            startTick = GenTicks.TicksGame;
            started = true;
            Log.Message("[CampfirePlayTest] PASS native ritual started at a fueled campfire.");
        }
    }
}
