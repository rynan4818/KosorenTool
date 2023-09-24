using KosorenTool.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace KosorenTool.Models
{
    public class KosorenToolUIManager : IInitializable, IDisposable
    {
        private bool _disposedValue;
        private StandardLevelDetailViewController _standardLevelDetail;
        private MainMenuViewController _mainMenuView;
        private PlayerDataModel _playerDataModel;
        private KosorenToolPlayData _playdata;
        public readonly int ViewCount = 30;
        public IDifficultyBeatmap _selectedBeatmap;
        public static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        public static readonly List<object> SortChoices = new List<object>() { "Sort by Score" , "Sort by Date" };
        public event Action<string> OnResultRefresh;

    public KosorenToolUIManager(StandardLevelDetailViewController standardLevelDetailViewController,
            MainMenuViewController mainMenuViewController, PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._standardLevelDetail = standardLevelDetailViewController;
            this._mainMenuView = mainMenuViewController;
            this._playerDataModel = playerDataModel;
            this._playdata = playdata;
        }

        public void StandardLevelDetail_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController arg1, IDifficultyBeatmap arg2)
        {
            if (arg1 != null && arg2 != null)
                BeatmapInfoUpdated(arg2);
        }
        public void StandardLevelDetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            if (arg1 != null && arg1.selectedDifficultyBeatmap != null)
                BeatmapInfoUpdated(arg1.selectedDifficultyBeatmap);
        }
        private void MainMenu_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            BeatmapInfoUpdated(null);
        }

        public void SetBeatSaviorDataSubmission(bool value)
        {
            if (PluginConfig.Instance.BeatSaviorTargeted)
                BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", value);
        }

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
        {
            if (this._selectedBeatmap == beatmap)
                return;
            if (beatmap != null)
                this._selectedBeatmap = beatmap;
            if (!ResultRefresh())
                this.OnResultRefresh?.Invoke(string.Empty);
        }

        public bool ResultRefresh()
        {
            if (this._selectedBeatmap == null)
                return false;
            var playerdata = this._playerDataModel.playerData;
            if (playerdata == null)
                return false;
            var records = this._playdata.GetRecords(_selectedBeatmap);
            if (records?.Count == 0)
                return false;
            _ = SetRecords(records, playerdata);
            return true;
        }

        public async Task SetRecords(List<Record> records, PlayerData playerdata)
        {
            List<Record> truncated = records.Take(ViewCount).ToList();
            var beatmapData = await _selectedBeatmap.GetBeatmapDataAsync(_selectedBeatmap.GetEnvironmentInfo(), playerdata.playerSpecificSettings);
            var notesCount = beatmapData.cuttableNotesCount;
            var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);
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
                var reactionTime = r.JD * 500 / _selectedBeatmap.noteJumpMovementSpeed;
                builder.Append($"<size=3.5><color=#ffff00ff> {r.JD:0.0}m {reactionTime:0}ms</color></size>");
                builder.AppendLine();
            }
            this.OnResultRefresh?.Invoke(builder.ToString());
        }

        public void Initialize()
        {
            if (PluginConfig.Instance.BeatSaviorTargeted)
                BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", PluginConfig.Instance.DisableSubmission);
            this._standardLevelDetail.didChangeDifficultyBeatmapEvent += StandardLevelDetail_didChangeDifficultyBeatmapEvent;
            this._standardLevelDetail.didChangeContentEvent += StandardLevelDetail_didChangeContentEvent;
            this._mainMenuView.didDeactivateEvent += MainMenu_didDeactivateEvent;
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
