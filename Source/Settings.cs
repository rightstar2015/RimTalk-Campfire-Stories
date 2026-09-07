using System;
using UnityEngine;
using Verse;

namespace CampfireStories
{
    public sealed class StorySettings : ModSettings
    {
        public int speakers = 0;
        public int replies = 2;
        public int timeout = 90;
        public int lineLength = 120;
        public bool avoidRecent = true;

        public string prompt = "";
        public override void ExposeData()
        {
            Scribe_Values.Look(ref speakers, "speakers", 0);
            Scribe_Values.Look(ref replies, "replies", 2);
            Scribe_Values.Look(ref timeout, "timeout", 90);
            Scribe_Values.Look(ref lineLength, "lineLength", 120);
            Scribe_Values.Look(ref avoidRecent, "avoidRecent", true);

            Scribe_Values.Look(ref prompt, "prompt", "");
            speakers = Mathf.Clamp(speakers, 0, 3);
            replies = Mathf.Clamp(replies, 0, 2);
            timeout = Mathf.Clamp(timeout, 30, 180);
            lineLength = Mathf.Clamp(lineLength, 40, 240);
        }
    }

    public sealed class StoryMod : Mod
    {
        public static StorySettings Settings;
        public StoryMod(ModContentPack content) : base(content) { Settings = GetSettings<StorySettings>(); }
        public override string SettingsCategory() { return "Rimtalk - " + "CS_Title".Translate(); }
        public override void DoSettingsWindowContents(Rect rect)
        {
            var list = new Listing_Standard();
            list.Begin(rect);
            list.Label((StoryIntegration.Ready ? (StoryIntegration.Expanded ? "CS_ModeExpanded" : "CS_ModeRimTalk") : "CS_ModeBase").Translate());
            list.Label("CS_Speakers".Translate() + ": " + (Settings.speakers == 0 ? "CS_Random".Translate().ToString() : Settings.speakers.ToString()));
            Settings.speakers = Mathf.RoundToInt(list.Slider(Settings.speakers, 0, 3));
            list.Label("CS_Replies".Translate() + ": " + Settings.replies);
            Settings.replies = Mathf.RoundToInt(list.Slider(Settings.replies, 0, 2));
            list.Label("CS_Timeout".Translate() + ": " + Settings.timeout);
            Settings.timeout = Mathf.RoundToInt(list.Slider(Settings.timeout, 30, 180));
            list.Label("CS_Length".Translate() + ": " + Settings.lineLength);
            Settings.lineLength = Mathf.RoundToInt(list.Slider(Settings.lineLength, 40, 240));
            list.CheckboxLabeled("CS_Recent".Translate(), ref Settings.avoidRecent);

            list.Label("CS_Prompt".Translate());
            if (list.ButtonText("CS_Reset".Translate())) Settings.prompt = "";
            Rect edit = list.GetRect(Math.Max(100, rect.height - list.CurHeight - 8));
            Settings.prompt = Widgets.TextArea(edit, string.IsNullOrEmpty(Settings.prompt) ? "CS_DefaultPrompt".Translate().ToString() : Settings.prompt);
            list.End();
        }
    }
}
