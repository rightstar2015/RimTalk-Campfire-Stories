using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RimTalk.Data;
using RimTalk.Prompt;
using RimTalk.Service;
using RimTalk.Source.Data;
using Verse;

namespace CampfireStories
{
    public sealed class RimTalkAdapter : IStoryService
    {
        // Called only by the explicit disposable quicktest; never in ordinary games.
        public void TestMemories(Pawn first, Pawn second)
        {
            if (!Environment.GetCommandLineArgs().Contains("-campfire-playtest") || !Environment.GetCommandLineArgs().Contains("-quicktest")) return;
            TalkHistory.AddMessageHistory(first, "PRIVATE_PROMPT_MUST_NOT_BE_SELECTED", "HISTORY_FIXTURE_SHARED_MEAL");
            TalkHistory.AddMessageHistory(second, "PRIVATE_PROMPT_MUST_NOT_BE_SELECTED", "OTHER_PAWN_HISTORY");
            if (StoryIntegration.Expanded)
            {
                var comp = first.AllComps.First(c => IsMemoryComponent(c.GetType()));
                var add = comp.GetType().GetMethod("AddActiveMemory");
                add.Invoke(comp, new object[] { "EXPAND_FIXTURE_PERSONAL_MEAL", Enum.Parse(add.GetParameters()[1].ParameterType, "Event"), 1f, null });
            }
            var memories = Read(first);
            string expected = StoryIntegration.Expanded ? "EXPAND_FIXTURE_PERSONAL_MEAL" : "HISTORY_FIXTURE_SHARED_MEAL";
            if (!memories.Any(m => m.Content.Contains(expected)) || memories.Any(m => m.Content.Contains("PRIVATE_PROMPT_MUST_NOT_BE_SELECTED") || m.Content.Contains("OTHER_PAWN_HISTORY")))
                throw new Exception("Personal memory source isolation failed.");
            if (StoryIntegration.Expanded && memories.Any(m => m.Content.Contains("HISTORY_FIXTURE_SHARED_MEAL"))) throw new Exception("Mixed memory sources.");
            Log.Message("[CampfirePlayTest] PASS personal memory source: " + (StoryIntegration.Expanded ? "Expand Memory" : "RimTalk history") + "; no prompts or other pawn memories selected.");
        }
        private static bool IsMemoryComponent(Type type)
        {
            while (type != null)
            {
                if (type.FullName == "RimTalk.Memory.FourLayerMemoryComp") return true;
                type = type.BaseType;
            }
            return false;
        }
        public bool Ready => RimTalk.Settings.Get().IsEnabled && RimTalk.Settings.Get().GetActiveConfig() != null;
        public bool Busy => AIService.IsBusy();
        public int DisplayWaitLimit => Math.Max(1800, RimTalk.Settings.Get().ReplyInterval * 60 + 600);
        public List<Recollection> Read(Pawn pawn)
        {
            var result = new List<Recollection>();
            if (StoryIntegration.Expanded)
            {
                var comp = pawn.AllComps.FirstOrDefault(c => IsMemoryComponent(c.GetType()));
                var entries = comp?.GetType().GetMethod("GetAllMemories")?.Invoke(comp, null) as IEnumerable;
                if (entries == null) return result;
                foreach (var entry in entries)
                {
                    if (entry == null) continue;
                    var type = entry.GetType();
                    string content = (type.GetField("Content")?.GetValue(entry) ?? type.GetProperty("Content")?.GetValue(entry, null)) as string;
                    string id = (type.GetField("Id")?.GetValue(entry) ?? type.GetProperty("Id")?.GetValue(entry, null)) as string;
                    result.Add(new Recollection { Id = "expand:" + (id ?? content), Content = content });
                }
                return result;
            }
            // Only past spoken text, never the user's prompts or provider configuration.
            foreach (var entry in TalkHistory.GetMessageHistory(pawn, true).Where(m => m.role == Role.AI))
                result.Add(new Recollection { Id = "rimtalk:" + entry.message, Content = "Past conversation this colonist participated in (not necessarily their own experience):\n" + entry.message });
            return result;
        }
        public bool CanDisplay(Pawn pawn) => Cache.Get(pawn)?.CanDisplayTalk() == true;
        public bool HasQueued(Pawn pawn)
        {
            var state = Cache.Get(pawn);
            if (state == null) return true;
            state.DrainIncomingTalkResponses();
            return state.TalkResponses.Count > 0;
        }
        public string Name(Pawn pawn, List<Pawn> participants) => PromptService.GetUniqueName(pawn, participants);
        public object Create(string prompt, List<Pawn> participants, int mapId)
        {
            var request = new TalkRequest(prompt, participants[0], participants.Skip(1).FirstOrDefault(), TalkType.User)
                { Participants = participants, MapId = mapId, IsMonologue = participants.Count == 1 };
            request.PromptMessages = PromptManager.Instance.BuildMessages(request, participants, "Sharing stories around a lit campfire during a cultural ritual.");
            return request;
        }
        public async Task Generate(object handle, Action<StoryLine> receive)
        {
            var request = (TalkRequest)handle;
            // RimTalk owns all network configuration and credentials.
            await AIService.ChatStreaming(request, response =>
            {
                var pawn = request.Participants.FirstOrDefault(p => response.Name == Name(p, request.Participants));
                if (pawn == null || string.IsNullOrWhiteSpace(response.Text)) return;
                receive(new StoryLine { pawn = pawn, text = response.Text.Substring(0, Math.Min(1000, response.Text.Length)), id = Guid.NewGuid().ToString() });
            });
        }
        public void Cancel(object request)
        { if (request != null && ReferenceEquals(AIService.CurrentRequest, request) && AIService.IsBusy()) AIService.CancelCurrent(); }
        public void Queue(StoryLine line)
        { Cache.Get(line.pawn)?.QueueIncomingResponse(new TalkResponse(TalkType.User, line.pawn.LabelShort, line.text) { Id = Guid.Parse(line.id) }); }
        public bool Spoken(string id) => TalkHistory.GetSpokenTick(Guid.Parse(id)) >= 0;
        public void Remove(StoryLine line)
        {
            if (line.pawn == null) return;
            var state = Cache.Get(line.pawn);
            if (state == null) return;
            state.DrainIncomingTalkResponses();
            state.TalkResponses.RemoveAll(r => r.Id.ToString() == line.id);
        }
    }
}
