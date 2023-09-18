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
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace KosorenTool.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable, IDisposable, IBeatmapInfoUpdater
    {
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        public IDifficultyBeatmap _selectedBeatmap;

        public static readonly HttpClient ScoresaberHttpClient = new HttpClient();
        internal static readonly BS_Utils.Utilities.Config BeatSaviorDataConfig = new BS_Utils.Utilities.Config("BeatSaviorData");
        private bool _disposedValue;
        private KosorenToolPlayData _playdata;
        private PlayerDataModel _playerDataModel;
        private string _reslut;
        private bool _recordClear;

        [Inject]
        public void Constractor(PlayerDataModel playerDataModel, KosorenToolPlayData playdata)
        {
            this._playdata = playdata;
            this._playerDataModel = playerDataModel;
        }

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
        {
            _selectedBeatmap = beatmap;
            RefreshResult();
        }

        public void RefreshResult()
        {
            bool CheckSetRecords()
            {
                if (_selectedBeatmap == null)
                    return false;
                var playerdata = _playerDataModel.playerData;
                if (playerdata == null)
                    return false;
                var records = _playdata.GetRecords(_selectedBeatmap);
                if (records?.Count == 0)
                    return false;
                _ = SetRecords(records, playerdata);
                return true;
            }
            this._recordClear = false;
            if (!CheckSetRecords())
            {
                this._recordClear = true;
                this._reslut = "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
                NotifyPropertyChanged(nameof(Result));
            }
        }

        public async Task GetRivalInfo(string songHash, string userID)
        {
            var playerFullInfoURL = $"https://scoresaber.com/api/player/{userID}/full";
        }


        public async Task SetRecords(List<Record> records, PlayerData playerdata)
        {
            List<Record> truncated = records.Take(20).ToList();
            var beatmapData = await _selectedBeatmap.GetBeatmapDataAsync(_selectedBeatmap.GetEnvironmentInfo(), playerdata.playerSpecificSettings);
            if (this._recordClear)
                return;
            var notesCount = beatmapData.cuttableNotesCount;
            var maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(beatmapData);
            var builder = new StringBuilder(1000);
            var enterSum = 0;

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
                enterSum++;
            }
            if (truncated.Count <= 20)
            {
                for (int i = truncated.Count; i <= 20; i++)
                {
                    enterSum++;
                    builder.AppendLine();
                }
            }
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
            GameplaySetup.instance.AddTab(Plugin.Name, this.ResourceName, this, MenuType.Solo | MenuType.Custom);
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
