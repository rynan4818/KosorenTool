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
        public virtual bool BeatSaviorTargeted { get; set; } = true;
        public virtual bool ShowFailed { get; set; } = true;
        public virtual string Sort { get; set; } = "Sort by Score";
        public virtual bool AllTimeSave { get; set; } = true;
    }
}
