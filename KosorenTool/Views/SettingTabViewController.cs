using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Zenject;
using System;
using System.Collections.Generic;
using TMPro;
using KosorenTool.Configuration;
using KosorenTool.Models;
using UnityEngine;
using Unity.Mathematics;

namespace KosorenTool.Views
{
    public class SettingTabViewController : IInitializable, IDisposable
    {
        private bool _disposedValue;
        private SettingsManager _settingsManager;
        private KosorenToolUIManager _kosorenToolUIManager;
        private KosorenInfoView _kosorenInfoView;
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        [UIComponent("Result")]
        public readonly TextMeshProUGUI _result;
        [UIValue("SortChoices")]
        public List<object> SortChoices { get; set; } = KosorenToolUIManager.SortChoices;

        public SettingTabViewController(SettingsManager settingsManager, KosorenToolUIManager kosorenToolUIManager, KosorenInfoView kosorenInfoView)
        {
            this._settingsManager = settingsManager;
            this._kosorenToolUIManager = kosorenToolUIManager;
            this._kosorenInfoView = kosorenInfoView;
        }

        public void OnResultRefresh(string value)
        {
            if (_result == null)
                return;
            _result.text = value;
        }

        [UIValue("DisableSubmission")]
        public bool DisableSubmission
        {
            get => PluginConfig.Instance.DisableSubmission;
            set
            {
                PluginConfig.Instance.DisableSubmission = value;
                this._kosorenInfoView.KosorenInfoChange(value);
            }
        }

        [UIValue("Sort")]
        public string SortByDate
        {
            get => PluginConfig.Instance.Sort;
            set
            {
                PluginConfig.Instance.Sort = value;
                this._kosorenToolUIManager.BeatmapInfoUpdated(new BeatmapKey(), null);
            }
        }

        [UIValue("ShowFailed")]
        public bool ShowFailed
        {
            get => PluginConfig.Instance.ShowFailed;
            set
            {
                PluginConfig.Instance.ShowFailed = value;
                this._kosorenToolUIManager.BeatmapInfoUpdated(new BeatmapKey(), null);
            }
        }

        [UIValue("AllTimeSave")]
        public bool AllTimeSave
        {
            get => PluginConfig.Instance.AllTimeSave;
            set => PluginConfig.Instance.AllTimeSave = value;
        }

        [UIValue("ScoreBelowPause")]
        public bool ScoreBelowPause
        {
            get => PluginConfig.Instance.ScoreBelowPause;
            set
            {
                PluginConfig.Instance.ScoreBelowPause = value;
                this._kosorenInfoView.ScoreBelowPause(value);
            }
        }

        [UIValue("SingleNotesScore")]
        public int SingleNotesScore
        {
            get => PluginConfig.Instance.SingleNotesScore;
            set => PluginConfig.Instance.SingleNotesScore = value;
        }

        [UIValue("AccuracyBelowPause")]
        public bool AccuracyBelowPause
        {
            get => PluginConfig.Instance.AccuracyBelowPause;
            set
            {
                PluginConfig.Instance.AccuracyBelowPause = value;
                this._kosorenInfoView.AccuracyBelowPause(value);
            }
        }

        [UIValue("MinimumAccuracy")]
        public float MinimumAccuracy
        {
            get => PluginConfig.Instance.MinimumAccuracy;
            set => PluginConfig.Instance.MinimumAccuracy = value;
        }

        [UIValue("StartUncheckedTime")]
        public int StartUncheckedTime
        {
            get => PluginConfig.Instance.StartUncheckedTime;
            set => PluginConfig.Instance.StartUncheckedTime = value;
        }

        [UIValue("ControllerZ")]
        public float ControllerZ
        {
            get => this._settingsManager.settings.controller.position.z * 100f;
            set => this._settingsManager.settings.controller.position = new float3(this._settingsManager.settings.controller.position.x, this._settingsManager.settings.controller.position.y, Mathf.Clamp(value / 100f, -0.1f, 0.1f));
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab(Plugin.Name, this.ResourceName, this, MenuType.Solo | MenuType.Custom | MenuType.Online);
            this._kosorenToolUIManager.OnResultRefresh += OnResultRefresh;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    GameplaySetup.instance?.RemoveTab(Plugin.Name);
                    this._kosorenToolUIManager.OnResultRefresh -= OnResultRefresh;
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
