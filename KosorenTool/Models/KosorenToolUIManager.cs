﻿using KosorenTool.Configuration;
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
        private PlayerDataModel _playerDataModel;
        private KosorenToolPlayData _playdata;
        public Queue<(Task, CancellationTokenSource)> _beatmapInfoUpdateQueue = new Queue<(Task, CancellationTokenSource)>();
        public readonly int ViewCount = 30;
        public IDifficultyBeatmap _selectedBeatmap;
        public static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        public static readonly List<object> SortChoices = new List<object>() { "Sort by Score" , "Sort by Date" , "Memo" };
        public event Action<string> OnResultRefresh;
        public string _memoFilePath;
        public static readonly string KosorenToolMemo = "KosorenToolMemo.txt";

    public KosorenToolUIManager(StandardLevelDetailViewController standardLevelDetailViewController,
            PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._standardLevelDetail = standardLevelDetailViewController;
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

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
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
                this.ResultRefreshQueueAdd(memo);
                return;
            }
            if (this._selectedBeatmap?.level?.levelID == beatmap?.level?.levelID &&
                this._selectedBeatmap.difficulty == beatmap.difficulty &&
                this._selectedBeatmap.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName == beatmap.parentDifficultyBeatmapSet?.beatmapCharacteristic?.serializedName)
                return;
            if (beatmap != null)
                this._selectedBeatmap = beatmap;
            this.ResultRefreshQueueAdd();
        }
        public void ResultRefreshQueueAdd(string result = null)
        {
            for (int i = 0; i < this._beatmapInfoUpdateQueue.Count; i++)
            {
                var (task, cts) = this._beatmapInfoUpdateQueue.Dequeue();
                cts?.Cancel();
                if (task != null && task.IsCompleted)
                {
                    cts?.Dispose();
                    task?.Dispose();
                }
                else
                {
                    this._beatmapInfoUpdateQueue.Enqueue((task, cts));
                }
            }
            if (result != null)
            {
                this.OnResultRefresh?.Invoke(result);
                return;
            }
            var cancellation = new CancellationTokenSource();
            this._beatmapInfoUpdateQueue.Enqueue((this.ResultRefresh(cancellation.Token), cancellation));
        }
        public async Task ResultRefresh(CancellationToken cancellationToken)
        {
            string result;
            try
            {
                result = await this.GetResult(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            this.OnResultRefresh?.Invoke(result);
        }

        public async Task<string> GetResult(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (this._selectedBeatmap == null)
                return string.Empty;
            var playerdata = this._playerDataModel.playerData;
            if (playerdata == null)
                return string.Empty;
            var records = this._playdata.GetRecords(this._selectedBeatmap);
            if (records?.Count == 0)
                return string.Empty;
            List<Record> truncated = records.Take(this.ViewCount).ToList();
            var beatmapData = await _selectedBeatmap.GetBeatmapDataAsync(_selectedBeatmap.GetEnvironmentInfo(), playerdata.playerSpecificSettings);
            cancellationToken.ThrowIfCancellationRequested();
            if (beatmapData == null)
                return string.Empty;
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
                var reactionTime = r.JD * 500 / this._selectedBeatmap.noteJumpMovementSpeed;
                builder.Append($"<size=3.5><color=#ffff00ff> {r.JD:0.0}m {reactionTime:0}ms</color></size>");
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public void Initialize()
        {
            this._standardLevelDetail.didChangeDifficultyBeatmapEvent += StandardLevelDetail_didChangeDifficultyBeatmapEvent;
            this._standardLevelDetail.didChangeContentEvent += StandardLevelDetail_didChangeContentEvent;
            this._playdata.OnBeatmapInfoUpdated += BeatmapInfoUpdated;
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
                    this._playdata.OnBeatmapInfoUpdated -= BeatmapInfoUpdated;
                    foreach (var (task, cts) in this._beatmapInfoUpdateQueue)
                    {
                        cts?.Cancel();
                        if (task != null && task.IsCompleted)
                        {
                            cts?.Dispose();
                            task?.Dispose();
                        }
                    }
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
