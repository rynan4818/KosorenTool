using KosorenTool.Configuration;
using System;
using Zenject;
using BS_Utils.Gameplay;
using BS_Utils.Utilities;

namespace KosorenTool.Models
{
    public class KosorenToolController : IInitializable, IDisposable
    {
        public bool _isPractice;
        private bool _disposedValue;
        private KosorenToolPlayData _playdata;
        public ResultsViewController ResultsViewController;
        public float _jumpDistance;
        public KosorenToolController(KosorenToolPlayData playdata)
        {
            this._playdata = playdata;
        }

        private void OnGameSceneLoaded()
        {
            var practiceSettings = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData?.practiceSettings;
            _isPractice = practiceSettings != null;
            if (PluginConfig.Instance.DisableSubmission)
                ScoreSubmission.DisableSubmission(Plugin.Name);
        }

        private void OnLevelFinished(object scene, LevelFinishedEventArgs eventArgs)
        {
            if (eventArgs.LevelType != LevelType.SoloParty)
                return;
            if (_isPractice || Gamemode.IsPartyActive)
                return;
            if (!PluginConfig.Instance.AllTimeSave && !PluginConfig.Instance.DisableSubmission)
                return;
            var result = ((LevelFinishedWithResultsEventArgs)eventArgs).CompletionResults;
            var beatmap = ((StandardLevelScenesTransitionSetupDataSO)scene)?.difficultyBeatmap;
            this._playdata.SaveRecord(beatmap, result, _jumpDistance);
        }

        public void Initialize()
        {
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            BSEvents.LevelFinished += OnLevelFinished;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    BSEvents.gameSceneLoaded -= OnGameSceneLoaded;
                    BSEvents.LevelFinished -= OnLevelFinished;
                    this._playdata.BackupPlaydata();
                }
                this._disposedValue = true;
            }
        }
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
