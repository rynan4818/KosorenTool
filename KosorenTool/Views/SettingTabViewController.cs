using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Zenject;
using System;
using System.Collections.Generic;
using TMPro;
using KosorenTool.Configuration;
using KosorenTool.Models;

namespace KosorenTool.Views
{
    public class SettingTabViewController : IInitializable, IDisposable
    {
        private bool _disposedValue;
        private KosorenToolUIManager _kosorenToolUIManager;
        private KosorenInfoView _kosorenInfoView;
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        [UIComponent("Result")]
        public readonly TextMeshProUGUI _result;
        [UIValue("SortChoices")]
        public List<object> SortChoices { get; set; } = KosorenToolUIManager.SortChoices;

        public SettingTabViewController(KosorenToolUIManager kosorenToolUIManager, KosorenInfoView kosorenInfoView)
        {
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
                this._kosorenToolUIManager.SetBeatSaviorDataSubmission(value);
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
                this._kosorenToolUIManager.BeatmapInfoUpdated(null);
            }
        }

        [UIValue("ShowFailed")]
        public bool ShowFailed
        {
            get => PluginConfig.Instance.ShowFailed;
            set
            {
                PluginConfig.Instance.ShowFailed = value;
                this._kosorenToolUIManager.BeatmapInfoUpdated(null);
            }
        }

        [UIValue("AllTimeSave")]
        public bool AllTimeSave
        {
            get => PluginConfig.Instance.AllTimeSave;
            set => PluginConfig.Instance.AllTimeSave = value;
        }

        [UIValue("BeatSaviorTargeted")]
        public bool BeatSaviorTargeted
        {
            get => PluginConfig.Instance.BeatSaviorTargeted;
            set
            {
                PluginConfig.Instance.BeatSaviorTargeted = value;
                if (value)
                    this._kosorenToolUIManager.SetBeatSaviorDataSubmission(PluginConfig.Instance.DisableSubmission);
            }
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
