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
        public bool _standardPlayerActive;
        public bool _kosorenModeActive;
        public bool _belowPauseModeActive;
        public KosorenToolController(KosorenToolPlayData playdata)
        {
            this._playdata = playdata;
        }

        public void OnLevelFinished(object scene, LevelFinishedEventArgs eventArgs)
        {
            if (eventArgs.LevelType == LevelType.Campaign || eventArgs.LevelType == LevelType.Tutorial)
                return;
            if (_isPractice || Gamemode.IsPartyActive)
                return;
            if (!PluginConfig.Instance.AllTimeSave && !PluginConfig.Instance.DisableSubmission)
                return;
            var result = ((LevelFinishedWithResultsEventArgs)eventArgs).CompletionResults;
            var setupData = (StandardLevelScenesTransitionSetupDataSO)scene;
            _ = this._playdata.SaveRecordAsync(setupData.beatmapKey, result, this._jumpDistance, this._kosorenModeActive);
        }

        public void Initialize()
        {
            BSEvents.LevelFinished += OnLevelFinished;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    BSEvents.LevelFinished -= OnLevelFinished;
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
