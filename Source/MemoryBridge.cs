using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace CampfireStories
{
    public sealed class StoryHistory : GameComponent
    {
        public List<string> recent = new List<string>();
        public StoryHistory(Game game) { }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref recent, "campfireRecent", LookMode.Value);
            if (recent == null) recent = new List<string>();
        }
        public void Remember(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            recent.Remove(key); recent.Add(key);
            while (recent.Count > 30) recent.RemoveAt(0);
        }
    }

    internal static class MemoryBridge
    {
        internal const string Marker = "[CampfireStories]";
        internal static List<Recollection> Read(Pawn pawn)
        {
            try
            {
                return (StoryIntegration.Service?.Read(pawn) ?? new List<Recollection>())
                    .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(Marker, StringComparison.Ordinal))
                    .GroupBy(m => m.Content).Select(g => g.First()).ToList();
            }
            catch (Exception ex) { Log.Warning("[CampfireStories] Memory read skipped: " + ex.GetType().Name); return new List<Recollection>(); }
        }
        internal static string Key(Pawn pawn, Recollection memory) { return pawn.ThingID + ":" + (memory.Id ?? memory.Content); }
        internal static Recollection Pick(Pawn pawn, HashSet<string> used)
        {
            var all = Read(pawn).Where(m => !used.Contains(m.Content)).ToList();
            var history = Current.Game.GetComponent<StoryHistory>();
            var fresh = StoryMod.Settings.avoidRecent ? all.Where(m => !history.recent.Contains(Key(pawn, m))).ToList() : all;
            return (fresh.Count > 0 ? fresh : all).RandomElementWithFallback();
        }
    }
}
