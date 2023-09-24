using KosorenTool.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;
using System;
using HarmonyLib;
using KosorenTool.HarmonyPatches;

namespace KosorenTool
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static Harmony _harmony;
        public const string HARMONY_ID = "com.github.rynan4818.KosorenTool";
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static string Name => "KosorenTool";
        public static event Action OnPluginExit;

        [Init]
        /// <summary>
        /// IPAによってプラグインが最初にロードされたときに呼び出される（ゲームが開始されたとき、またはプラグインが無効な状態で開始された場合は有効化されたときのいずれか）
        /// [Init]コンストラクタを使用するメソッドや、InitWithConfigなどの通常のメソッドの前に呼び出されるメソッド
        /// [Init]は1つのコンストラクタにのみ使用してください
        /// </summary>
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("KosorenTool initialized.");
            _harmony = new Harmony(HARMONY_ID);

            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");

            zenjector.Install<KosorenToolAppInstaller>(Location.App);
            zenjector.Install<KosorenToolMenuInstaller>(Location.Menu);
            zenjector.Install<KosorenToolPlayerInstaller>(Location.Player);
            zenjector.Install<KosorenToolStandardPlayerInstaller>(Location.StandardPlayer);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
            var orginal = AccessTools.Method("TournamentAssistant.Behaviors.ScoreMonitor:Awake");
            var postfix = AccessTools.Method(typeof(ScoreMonitorPatch), nameof(ScoreMonitorPatch.AwakePostfix));
            if (orginal != null)
            {
                Log.Debug("TournamentAssistant Patch Load");
                _harmony.Patch(orginal, null, new HarmonyMethod(postfix));
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            OnPluginExit?.Invoke();
            _harmony?.UnpatchSelf();
        }
    }
}
