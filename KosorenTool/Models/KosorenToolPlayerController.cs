using BS_Utils.Gameplay;
using KosorenTool.Configuration;
using KosorenTool.HarmonyPatches;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using KosorenTool.Util;
using HMUI;

namespace KosorenTool.Models
{
    public class KosorenToolPlayerController : IInitializable, IDisposable, ICutScoreBufferDidFinishReceiver
    {
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private AudioTimeSyncController _audioTimeSyncController;
        private KosorenToolController _kosorenToolController;
        private ScoreController _scoreController;
        private PauseController _pauseController;
        private bool _initializeError;
        private bool disposedValue;
        private readonly CancellationTokenSource connectionClosed = new CancellationTokenSource();
        public GameObject _canvasObject;
        public CurvedTextMeshPro _canvasTextPro;

        public static readonly Vector2 CanvasSize = new Vector2(50, 10);
        public static readonly Vector3 Scale = new Vector3(0.01f, 0.01f, 0.01f);
        public static readonly Vector3 LeftPosition = new Vector3(0, 2.1f, 2.2f);
        public static readonly Vector3 LeftRotation = new Vector3(0, 0, 0);

        public KosorenToolPlayerController(DiContainer container)
        {
            this._initializeError = true;
            try
            {
                this._beatmapObjectSpawnController = container.Resolve<BeatmapObjectSpawnController>();
                this._audioTimeSyncController = container.Resolve<AudioTimeSyncController>();
                this._kosorenToolController = container.Resolve<KosorenToolController>();
                this._scoreController = container.Resolve<ScoreController>();
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
                return;
            }
            this._pauseController = container.TryResolve<PauseController>();
            this._initializeError = false;
        }

        public void Initialize()
        {
            if (this._initializeError)
                return;
            this._kosorenToolController._isPractice = true;
            _ = this.SongStartWait();
        }

        public async Task SongStartWait()
        {
            this._kosorenToolController._jumpDistance = 0;
            this._kosorenToolController._kosorenModeActive = false;
            this._kosorenToolController._scoreBelowPauseModeActive = false;
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
            if (this._kosorenToolController._standardPlayerActive && PluginConfig.Instance.ScoreBelowPause && !ScoreMonitorPatch.TournamentAssistantActive)
            {
                this._kosorenToolController._scoreBelowPauseModeActive = true;
                if (this._scoreController != null && this._pauseController != null)
                    this._scoreController.scoringForNoteStartedEvent += this.OnScoringForNoteStarted;
                if (this._pauseController != null)
                {
                    this._pauseController.didPauseEvent += this.OnGamePause;
                    this._pauseController.didResumeEvent += this.OnGameResume;
                }
                this._canvasObject = new GameObject("Last Note Score Canvas", typeof(Canvas), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                var sizeFitter = this._canvasObject.GetComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                var canvas = this._canvasObject.GetComponent<Canvas>();
                canvas.sortingOrder = 3;
                canvas.renderMode = RenderMode.WorldSpace;
                var rectTransform = canvas.transform as RectTransform;
                rectTransform.sizeDelta = CanvasSize;
                this._canvasObject.transform.position = LeftPosition + new Vector3(PluginConfig.Instance.InfoXoffset, PluginConfig.Instance.InfoYoffset, PluginConfig.Instance.InfoZoffset);
                this._canvasObject.transform.eulerAngles = LeftRotation;
                this._canvasObject.transform.localScale = Scale;
                this._canvasTextPro = Utility.CreateText(canvas.transform as RectTransform, string.Empty, new Vector2(10, 31));
                rectTransform = this._canvasTextPro.transform as RectTransform;
                rectTransform.SetParent(canvas.transform, false);
                rectTransform.anchoredPosition = Vector2.zero;
                this._canvasTextPro.fontSize = PluginConfig.Instance.ViewFontSize;
                this._canvasTextPro.color = Color.white;
                this._canvasObject.SetActive(false);
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
                    if (this._kosorenToolController._scoreBelowPauseModeActive)
                    {
                        UnityEngine.Object.Destroy(this._canvasObject);
                        if (this._scoreController != null)
                            this._scoreController.scoringForNoteStartedEvent -= this.OnScoringForNoteStarted;
                        if (this._pauseController != null)
                        {
                            this._pauseController.didPauseEvent -= this.OnGamePause;
                            this._pauseController.didResumeEvent -= this.OnGameResume;
                        }
                    }
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

        /// <summary>
        /// スコア変化のあるノーツカットイベント発生時
        /// </summary>
        /// <param name="scoringElement"></param>
        public void OnScoringForNoteStarted(ScoringElement scoringElement)
        {
            if (!this._kosorenToolController._scoreBelowPauseModeActive)
                return;
            switch (scoringElement)
            {
                case GoodCutScoringElement goodCut:
                    var noteData = goodCut.noteData;
                    var cutScoreBuffer = goodCut.cutScoreBuffer;
                    if (cutScoreBuffer != null && noteData.gameplayType != NoteData.GameplayType.Bomb && goodCut.cutScoreBuffer.noteCutInfo.allIsOK && noteData.scoringType != NoteData.ScoringType.BurstSliderElement)
                        cutScoreBuffer.RegisterDidFinishReceiver(this);
                    break;
            }
        }

        /// <summary>
        /// ノーツの正常カット時の点数計算完了イベント発生時
        /// </summary>
        /// <param name="csb"></param>
        public void HandleCutScoreBufferDidFinish(CutScoreBuffer csb)
        {
            if (!this._kosorenToolController._scoreBelowPauseModeActive)
                return;
            csb.UnregisterDidFinishReceiver(this);
            if (this._pauseController != null && PluginConfig.Instance.ScoreBelowPause && (csb.beforeCutScore + csb.afterCutScore + csb.centerDistanceCutScore) <= PluginConfig.Instance.SingleNotesScore)
            {
                this._canvasTextPro.text = $"{csb.beforeCutScore} + {csb.afterCutScore} + {csb.centerDistanceCutScore} = {csb.beforeCutScore + csb.afterCutScore + csb.centerDistanceCutScore}";
                this._pauseController.Pause();
            }
        }

        /// <summary>
        /// Pauseイベント発生時
        /// </summary>
        public void OnGamePause()
        {
            this._canvasObject.SetActive(true);
        }

        /// <summary>
        /// PauseからContinueイベント発生時
        /// </summary>
        public void OnGameResume()
        {
            if (!this._kosorenToolController._scoreBelowPauseModeActive)
                return;
            this._canvasObject.SetActive(false);
        }
    }
}
