using HMUI;
using KosorenTool.Interfaces;
using System;
using System.Collections.Generic;
using Zenject;
using IPA.Utilities;
using System.Windows.Forms;

namespace KosorenTool.Models
{
    public class KosorenToolUIManager : IInitializable, IDisposable
    {
        private bool _disposedValue;
        private StandardLevelDetailViewController _standardLevelDetail;
        private MainMenuViewController _mainMenuViewController;
        private readonly List<IBeatmapInfoUpdater> _beatmapInfoUpdaters;

        public KosorenToolUIManager(StandardLevelDetailViewController standardLevelDetailViewController, MainMenuViewController mainMenuViewController, List<IBeatmapInfoUpdater> iBeatmapInfoUpdaters)
        {
            _standardLevelDetail = standardLevelDetailViewController;
            _beatmapInfoUpdaters = iBeatmapInfoUpdaters;
            _mainMenuViewController = mainMenuViewController;
        }

        public void StandardLevelDetail_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController arg1, IDifficultyBeatmap arg2)
        {
            Plugin.Log.Info("StandardLevelDetail_didChangeDifficultyBeatmapEvent");
            if (arg1 != null && arg2 != null)
                DiffcultyBeatmapUpdated(arg2);
        }
        public void StandardLevelDetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            Plugin.Log.Info("StandardLevelDetail_didChangeContentEvent");
            if (arg1 != null && arg1.selectedDifficultyBeatmap != null)
                DiffcultyBeatmapUpdated(arg1.selectedDifficultyBeatmap);
        }
        public void MainMenu_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            Plugin.Log.Info("MainMenu_didDeactivateEvent");
            foreach (var beatmapInfoUpdater in _beatmapInfoUpdaters)
                beatmapInfoUpdater.RefreshResult();
        }
        private void DiffcultyBeatmapUpdated(IDifficultyBeatmap difficultyBeatmap)
        {
            foreach (var beatmapInfoUpdater in _beatmapInfoUpdaters)
                beatmapInfoUpdater.BeatmapInfoUpdated(difficultyBeatmap);
        }
        public void Initialize()
        {
            _standardLevelDetail.didChangeDifficultyBeatmapEvent += StandardLevelDetail_didChangeDifficultyBeatmapEvent;
            _standardLevelDetail.didChangeContentEvent += StandardLevelDetail_didChangeContentEvent;
            _mainMenuViewController.didDeactivateEvent += MainMenu_didDeactivateEvent;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    _standardLevelDetail.didChangeDifficultyBeatmapEvent -= StandardLevelDetail_didChangeDifficultyBeatmapEvent;
                    _standardLevelDetail.didChangeContentEvent -= StandardLevelDetail_didChangeContentEvent;
                    _mainMenuViewController.didDeactivateEvent -= MainMenu_didDeactivateEvent;
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
