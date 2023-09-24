using BS_Utils.Gameplay;
using KosorenTool.Configuration;
using KosorenTool.HarmonyPatches;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace KosorenTool.Models
{
    public class KosorenToolPlayerController : IInitializable, IDisposable
    {
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private AudioTimeSyncController _audioTimeSyncController;
        private KosorenToolController _kosorenToolController;
        private bool disposedValue;
        private readonly CancellationTokenSource connectionClosed = new CancellationTokenSource();
        public KosorenToolPlayerController(BeatmapObjectSpawnController beatmapObjectSpawnController, AudioTimeSyncController audioTimeSyncController, KosorenToolController kosorenToolController)
        {
            this._beatmapObjectSpawnController = beatmapObjectSpawnController;
            this._audioTimeSyncController = audioTimeSyncController;
            this._kosorenToolController = kosorenToolController;
        }

        public void Initialize()
        {
            this._kosorenToolController._isPractice = true;
            _ = this.SongStartWait();
        }

        public async Task SongStartWait()
        {
            this._kosorenToolController._jumpDistance = 0;
            this._kosorenToolController._kosorenModeActive = false;
            var songTime = this._audioTimeSyncController.songTime;
            var token = connectionClosed.Token;
            try
            {
                while (this._audioTimeSyncController.songTime <= songTime)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(500);
                }
            }
            catch (Exception)
            {
                return;
            }
            this._kosorenToolController._jumpDistance = this._beatmapObjectSpawnController.jumpDistance;
            var practiceSettings = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData?.practiceSettings;
            this._kosorenToolController._isPractice = practiceSettings != null;
            if (!this._kosorenToolController._isPractice && this._kosorenToolController._standardPlayerActive && !Gamemode.IsPartyActive && PluginConfig.Instance.DisableSubmission && !ScoreMonitorPatch.TournamentAssistantActive)
            {
                this._kosorenToolController._kosorenModeActive = true;
                ScoreSubmission.DisableSubmission(Plugin.Name);
                Plugin.Log.Info("KOSOREN mode enabled");
            }
            Plugin.Log.Info("SongStart");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.connectionClosed.Cancel();
                    this._kosorenToolController._standardPlayerActive = false;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
