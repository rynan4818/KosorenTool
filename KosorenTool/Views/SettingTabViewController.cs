using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using KosorenTool.Configuration;
using KosorenTool.Models;
using KosorenTool.Interfaces;
using Zenject;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using TMPro;

namespace KosorenTool.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable, IDisposable, IBeatmapInfoUpdater
    {
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        public IDifficultyBeatmap _selectedBeatmap;

        internal static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        public readonly int ViewCount = 22;
        private bool _disposedValue;
        private KosorenToolPlayData _playdata;
        private PlayerDataModel _playerDataModel;
        [UIComponent("Result")]
        public readonly TextMeshProUGUI _result;

        [Inject]
        public void Constractor(PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._playdata = playdata;
            this._playerDataModel = playerDataModel;
        }

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
        {
            if (beatmap != null)
                this._selectedBeatmap = beatmap;
            var builder = new StringBuilder(200);
            for (int i = 0; i <= ViewCount; i++)
                builder.AppendLine();
            this._result.text = builder.ToString();
            ResultRefresh();
        }

        public bool ResultRefresh()
        {
            if (_selectedBeatmap == null)
                return false;
            var playerdata = _playerDataModel.playerData;
            if (playerdata == null)
                return false;
            var records = _playdata.GetRecords(_selectedBeatmap);
            if (records?.Count == 0)
                return false;
            _= SetRecords(records, playerdata);
            return true;
        }

        public async Task SetRecords(List<Record> records, PlayerData playerdata)
        {
            List<Record> truncated = records.Take(ViewCount).ToList();
            var beatmapData = await _selectedBeatmap.GetBeatmapDataAsync(_selectedBeatmap.GetEnvironmentInfo(), playerdata.playerSpecificSettings);
            var notesCount = beatmapData.cuttableNotesCount;
            var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);
            var builder = new StringBuilder(200);

            string Space(int len)
            {
                var space = string.Concat(Enumerable.Repeat("_", len));
                return $"<size=1><color=#00000000>{space}</color></size>";
            }
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
                builder.Append(Space(truncated.Count - truncated.IndexOf(r) - 1));
                builder.Append($"<size=2.5><color=#696969ff>{localDateTime:d}</color></size>");
                builder.Append($"<size=3.5><color=#2f4f4fff> {r.ModifiedScore}</color></size>");
                builder.Append($"<size=3.5><color=#fffacdff> {accuracy:0.00}%</color></size>");

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
                builder.Append(Space(truncated.IndexOf(r)));
                builder.AppendLine();
            }
            if (truncated.Count < ViewCount)
            {
                for (int i = truncated.Count; i <= ViewCount; i++)
                    builder.AppendLine();
            }
            this._result.text = builder.ToString();
        }

        [UIValue("DisableSubmission")]
        public bool DisableSubmission
        {
            get => PluginConfig.Instance.DisableSubmission;
            set
            {
                PluginConfig.Instance.DisableSubmission = value;
                if (PluginConfig.Instance.BeatSaviorTargeted)
                    BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", value);
            }
        }

        [UIValue("SortByDate")]
        public bool SortByDate
        {
            get => PluginConfig.Instance.SortByDate;
            set
            {
                PluginConfig.Instance.SortByDate = value;
                this.BeatmapInfoUpdated(null);
            }
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }
        public void Initialize()
        {
            if (PluginConfig.Instance.BeatSaviorTargeted)
                BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", PluginConfig.Instance.DisableSubmission);
            GameplaySetup.instance.AddTab(Plugin.Name, this.ResourceName, this, MenuType.Solo);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                        GameplaySetup.instance?.RemoveTab(Plugin.Name);
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
