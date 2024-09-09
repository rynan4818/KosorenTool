using BS_Utils.Utilities;
using KosorenTool.Configuration;
using ModestTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace KosorenTool.Models
{
    public class KosorenToolUIManager : IInitializable, IDisposable
    {
        private bool _disposedValue;
        private StandardLevelDetailViewController _standardLevelDetail;
        private MissionSelectionMapViewController _missionSelectionMapViewController;
        private MainMenuViewController _mainMenuView;
        private BeatmapLevelsModel _beatmapLevelsModel;
        private BeatmapDataLoader _beatmapDataLoader;
        private PlayerDataModel _playerDataModel;
        private KosorenToolPlayData _playdata;
        public CancellationTokenSource _setRecordsClosed;
        public readonly int ViewCount = 30;
        public (BeatmapKey, BeatmapLevel) _selectedBeatmap;
        public static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        public static readonly List<object> SortChoices = new List<object>() { "Sort by Score" , "Sort by Date" , "Memo" };
        public event Action<string> OnResultRefresh;
        public string _memoFilePath;
        public static readonly string KosorenToolMemo = "KosorenToolMemo.txt";

        public KosorenToolUIManager(StandardLevelDetailViewController standardLevelDetailViewController, MissionSelectionMapViewController missionSelectionMapViewController,
            MainMenuViewController mainMenuViewController, BeatmapLevelsModel beatmapLevelsModel, BeatmapDataLoader beatmapDataLoader, PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._standardLevelDetail = standardLevelDetailViewController;
            this._missionSelectionMapViewController = missionSelectionMapViewController;
            this._mainMenuView = mainMenuViewController;
            this._beatmapLevelsModel = beatmapLevelsModel;
            this._beatmapDataLoader = beatmapDataLoader;
            this._playerDataModel = playerDataModel;
            this._playdata = playdata;
        }

        public void StandardLevelDetail_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController arg1)
        {
            if (arg1 != null && arg1.beatmapLevel != null)
                BeatmapInfoUpdated(arg1.beatmapKey, arg1.beatmapLevel);
        }
        public void StandardLevelDetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            if (arg1 != null && arg1.beatmapLevel != null)
                BeatmapInfoUpdated(arg1.beatmapKey, arg1.beatmapLevel);
        }
        private void MainMenu_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            BeatmapInfoUpdated(new BeatmapKey(), null);
        }

        public void OnMenuSceneActive()
        {
            BeatmapInfoUpdated(new BeatmapKey(), null);
        }

        public void SetBeatSaviorDataSubmission(bool value)
        {
            if (PluginConfig.Instance.BeatSaviorTargeted)
                BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", value);
        }

        public void BeatmapInfoUpdated(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel)
        {
            if (PluginConfig.Instance.Sort == "Memo")
            {
                string memo;
                try
                {
                    memo = File.ReadAllText(this._memoFilePath);
                }
                catch (Exception e)
                {
                    Plugin.Log?.Error(e.ToString());
                    memo = "!!Memo File Read Error!!";
                }
                this.OnResultRefresh?.Invoke(memo);
                return;
            }
            if (this._selectedBeatmap == (beatmapKey, beatmapLevel))
                return;
            this._selectedBeatmap = (beatmapKey, beatmapLevel);
            if (!this.ResultRefresh())
            {
                this._setRecordsClosed?.Cancel();
                this.OnResultRefresh?.Invoke(string.Empty);
            }
        }

        public bool ResultRefresh()
        {
            if (this._selectedBeatmap.Item2 == null)
                return false;
            if (!this._selectedBeatmap.Item1.IsValid())
                return false;
            var playerdata = this._playerDataModel.playerData;
            if (playerdata == null)
                return false;
            var records = this._playdata.GetRecords(this._selectedBeatmap);
            if (records?.Count == 0)
                return false;
            _ = this.SetRecords(records, playerdata);
            return true;
        }

        public async Task SetRecords(List<Record> records, PlayerData playerdata)
        {
            this._setRecordsClosed = new CancellationTokenSource();
            var token = this._setRecordsClosed.Token;
            List<Record> truncated = records.Take(ViewCount).ToList();
            var beatmapData = await this.GetBeatmapDataAsync(this._selectedBeatmap.Item1, this._selectedBeatmap.Item2);
            if (token.IsCancellationRequested)
            {
                this._setRecordsClosed.Dispose();
                this._setRecordsClosed = null;
                return;
            }
            var basicBeatmapData = beatmapData.Item2.GetDifficultyBeatmapData(this._selectedBeatmap.Item1.beatmapCharacteristic, this._selectedBeatmap.Item1.difficulty);
            var notesCount = beatmapData.Item1.cuttableNotesCount;
            var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData.Item1);
            var builder = new StringBuilder(200);

            foreach (var r in truncated)
            {
                var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(r.Date).LocalDateTime;
                var levelFinished = r.LastNote < 0;
                var accuracy = r.RawScore / (float)maxScore * 100f;
                var param = _playdata.ConcatParam((Param)r.Param);
                if (param.Length == 0 && r.RawScore != r.ModifiedScore)
                {
                    param = "?!";
                }
                var notesRemaining = notesCount - r.LastNote;
                builder.Append($"<size=2.5><color=#696969ff>{localDateTime:d}</color></size>");
                builder.Append($"<size=3.5><color=#2f4f4fff> {r.ModifiedScore}</color></size>");

                if (r.Miss == "FC")
                {
                    builder.Append($"<size=3.5><color=#e6b422ff> {r.Miss}</color></size>");
                }
                else
                {
                    builder.Append($"<size=3.5><color=#ab031fff> {r.Miss}miss</color></size>");
                }

                if (levelFinished)
                {
                    // only display acc if the record is a finished level
                    builder.Append($"<size=3.5><color=#fffacdff> {accuracy:0.00}%</color></size>");
                }
                if (param.Length > 0)
                {
                    builder.Append($"<size=2><color=#e6b422ff> {param}</color></size>");
                }
                if (r.LastNote == -1)
                    builder.Append($"<size=2.5><color=#00bfffff> cleared</color></size>");
                else if (r.LastNote == 0) // old record (success, fail, or practice)
                    builder.Append($"<size=2.5><color=#584153ff> unknown</color></size>");
                else
                    builder.Append($"<size=2.5><color=#ff5722ff> +{notesRemaining} notes</color></size>");
                var reactionTime = r.JD * 500 / basicBeatmapData.noteJumpMovementSpeed;
                builder.Append($"<size=3.5><color=#ffff00ff> {r.JD:0.0}m {reactionTime:0}ms</color></size>");
                builder.AppendLine();
            }
            this.OnResultRefresh?.Invoke(builder.ToString());
        }

        public async Task<(IReadonlyBeatmapData, BeatmapLevel)> GetBeatmapDataAsync(BeatmapKey beatmapKey, BeatmapLevel beatmapLevel = null, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (beatmapLevel == null)
            {
                beatmapLevel = this._beatmapLevelsModel.GetBeatmapLevel(beatmapKey.levelId);
                if (beatmapLevel == null)
                    throw new Exception("Failed to get BeatmapLevel.");
            }
            var loadResult = await this._beatmapLevelsModel.LoadBeatmapLevelDataAsync(beatmapKey.levelId, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (loadResult.isError)
                throw new Exception("Failed to load beat map level data.");
            var beatmapLevelData = loadResult.beatmapLevelData;
            var beatmapData = await this._beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData, beatmapKey, beatmapLevel.beatsPerMinute, false, null, null, null, false);
            cancellationToken.ThrowIfCancellationRequested();
            return (beatmapData, beatmapLevel);
        }


        public void Initialize()
        {
            if (PluginConfig.Instance.BeatSaviorTargeted)
                BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", PluginConfig.Instance.DisableSubmission);
            this._standardLevelDetail.didChangeDifficultyBeatmapEvent += StandardLevelDetail_didChangeDifficultyBeatmapEvent;
            this._standardLevelDetail.didChangeContentEvent += StandardLevelDetail_didChangeContentEvent;
            this._mainMenuView.didDeactivateEvent += MainMenu_didDeactivateEvent;
            BSEvents.menuSceneActive += OnMenuSceneActive;
            this._memoFilePath = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, KosorenToolMemo);
            if (!File.Exists(this._memoFilePath))
            {
                try
                {
                    File.WriteAllText(this._memoFilePath, "");
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error(ex.ToString());
                }
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this._standardLevelDetail.didChangeDifficultyBeatmapEvent -= StandardLevelDetail_didChangeDifficultyBeatmapEvent;
                    this._standardLevelDetail.didChangeContentEvent -= StandardLevelDetail_didChangeContentEvent;
                    this._mainMenuView.didDeactivateEvent -= MainMenu_didDeactivateEvent;
                    BSEvents.menuSceneActive -= OnMenuSceneActive;
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
