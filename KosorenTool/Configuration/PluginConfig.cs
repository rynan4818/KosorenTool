using System.IO;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace KosorenTool.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public static readonly string DefaultPlayDataFile = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "KosorenPlayData.json");

        public virtual string PlayDataFile { get; set; } = DefaultPlayDataFile;
        public virtual bool DisableSubmission { get; set; } = false;
        public virtual bool ScoreBelowPause { get; set; } = false;
        public virtual int SingleNotesScore { get; set; } = 110;
        public virtual bool AccuracyBelowPause { get; set; } = false;
        public virtual float MinimumAccuracy { get; set; } = 95;
        public virtual int StartUncheckedTime { get; set; } = 20;
        public virtual bool BeatSaviorTargeted { get; set; } = true;
        public virtual bool ShowFailed { get; set; } = true;
        public virtual string Sort { get; set; } = "Sort by Score";
        public virtual bool AllTimeSave { get; set; } = true;
        public virtual float InfoXoffset { get; set; } = 0;
        public virtual float InfoYoffset { get; set; } = 0;
        public virtual float InfoZoffset { get; set; } = 0;
        public virtual float ViewFontSize { get; set; } = 20f;
    }
}
