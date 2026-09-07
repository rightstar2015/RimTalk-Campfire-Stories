using RimWorld;
using Verse;

namespace CampfireStories
{
    public sealed class RitualIconUpgrade : GameComponent
    {
        public const string IconPath = "UI/Icons/Rituals/CS_CampfireStories";
        public RitualIconUpgrade(Game game) { }
        public override void LoadedGame() { Upgrade(); }
        public override void StartedNewGame() { Upgrade(); }

        internal static void UpgradeIcon(Precept_Ritual ritual)
        {
            // Older saves retain the original festival override separately from the definition.
            if (ritual?.def?.defName == "CS_CampfireStories"
                && (string.IsNullOrEmpty(ritual.iconPathOverride)
                    || ritual.iconPathOverride == "UI/Icons/Rituals/RitualFestival"))
                ritual.iconPathOverride = IconPath;
        }

        private static void Upgrade()
        {
            if (Find.IdeoManager == null) return;
            foreach (var ideo in Find.IdeoManager.IdeosListForReading)
                foreach (var precept in ideo.PreceptsListForReading)
                    UpgradeIcon(precept as Precept_Ritual);
        }
    }
}
