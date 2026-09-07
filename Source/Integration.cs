using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verse;

namespace CampfireStories
{
    public sealed class Recollection { public string Id; public string Content; }
    public interface IStoryService
    {
        bool Ready { get; }
        bool Busy { get; }
        int DisplayWaitLimit { get; }
        List<Recollection> Read(Pawn pawn);
        bool CanDisplay(Pawn pawn);
        bool HasQueued(Pawn pawn);
        string Name(Pawn pawn, List<Pawn> participants);
        object Create(string prompt, List<Pawn> participants, int mapId);
        Task Generate(object request, Action<StoryLine> receive);
        void Cancel(object request);
        void Queue(StoryLine line);
        bool Spoken(string id);
        void Remove(StoryLine line);
    }
    public static class StoryIntegration
    {
        private static bool checkedService;
        private static IStoryService service;
        public static bool Expanded => ModsConfig.IsActive("cj.rimtalk.expandmemory");
        public static IStoryService Service
        {
            get
            {
                if (checkedService) return service;
                checkedService = true;
                if (!ModsConfig.IsActive("cj.rimtalk")) return null;
                try
                {
                    var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("CampfireStories.RimTalkAdapter", false)).FirstOrDefault(t => t != null);
                    if (type != null) service = (IStoryService)Activator.CreateInstance(type);
                }
                catch (Exception ex) { Log.Warning("[CampfireStories] Optional integration unavailable: " + ex.GetType().Name); }
                return service;
            }
        }
        public static bool Ready
        {
            get { try { return Service?.Ready == true; } catch { return false; } }
        }
    }
}
