using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using KosorenTool.Configuration;
using KosorenTool.Models;
using KosorenTool.Interfaces;
using Zenject;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KosorenTool.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable, IDisposable, IBeatmapInfoUpdater
    {
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        public IDifficultyBeatmap _selectedBeatmap;

        internal static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        private bool _disposedValue;
        private KosorenToolPlayData _playdata;
        private PlayerDataModel _playerDataModel;
        private string _reslut;

        [Inject]
        public void Constractor(PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._playdata = playdata;
            this._playerDataModel = playerDataModel;
        }

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
        {
            _selectedBeatmap = beatmap;
            if (!ResultRefresh())
            {
                this._reslut = "";
                NotifyPropertyChanged(nameof(Result));
            }
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
            SetRecords(records, playerdata);
            return true;
        }

        public async void SetRecords(List<Record> records, PlayerData playerdata)
        {
            List<Record> truncated = records.Take(10).ToList();
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
                builder.Append($"<size=2.5><color=#1a252bff>{localDateTime:d}</color></size>");
                builder.Append($"<size=3.5><color=#0f4c75ff> {r.ModifiedScore}</color></size>");
                builder.Append($"<size=3.5><color=#368cc6ff> {accuracy:0.00}%</color></size>");

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
                    builder.Append($"<size=3.5><color=#368cc6ff> {accuracy:0.00}%</color></size>");

                }
                if (param.Length > 0)
                {
                    builder.Append($"<size=2><color=#1a252bff> {param}</color></size>");
                }
                if (r.LastNote == -1)
                    builder.Append($"<size=2.5><color=#1a252bff> cleared</color></size>");
                else if (r.LastNote == 0) // old record (success, fail, or practice)
                    builder.Append($"<size=2.5><color=#584153ff> unknown</color></size>");
                else
                    builder.Append($"<size=2.5><color=#ff5722ff> +{notesRemaining} notes</color></size>");
                builder.Append(Space(truncated.IndexOf(r)));
                builder.AppendLine();
            }
            this._reslut = builder.ToString();
            Plugin.Log.Debug(_reslut);
            NotifyPropertyChanged(nameof(Result));
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

        [UIValue("result")]
        public string Result => this._reslut;

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
