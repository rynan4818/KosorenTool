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
        public virtual bool SortByDate { get; set; } = true;
        public virtual bool AllTimeSave { get; set; } = true;

        /// <summary>
        /// これは、BSIPAが設定ファイルを読み込むたびに（ファイルの変更が検出されたときを含めて）呼び出されます
        /// </summary>
        public virtual void OnReload()
        {
            // 設定ファイルを読み込んだ後の処理を行う
        }

        /// <summary>
        /// これを呼び出すと、BSIPAに設定ファイルの更新を強制します。 これは、ファイルが変更されたことをBSIPAが検出した場合にも呼び出されます。
        /// </summary>
        public virtual void Changed()
        {
            // 設定が変更されたときに何かをします
        }

        /// <summary>
        /// これを呼び出して、BSIPAに値を<paramref name ="other"/>からこの構成にコピーさせます。
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // このインスタンスのメンバーは他から移入されました
        }
    }
}
