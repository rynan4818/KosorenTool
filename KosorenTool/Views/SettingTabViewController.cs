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

namespace KosorenTool.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable, IDisposable, IBeatmapInfoUpdater
    {
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
        public IDifficultyBeatmap _selectedBeatmap;

        private bool _disposedValue;

        public void BeatmapInfoUpdated(IDifficultyBeatmap beatmap)
        {
            _selectedBeatmap = beatmap;
            NotifyPropertyChanged("result");
        }

        [UIValue("DisableSubmission")]
        public bool DisableSubmission
        {
            get => PluginConfig.Instance.DisableSubmission;
            set
            {
                PluginConfig.Instance.DisableSubmission = value;
                Plugin.BeatSaviorDataConfig.SetBool("BeatSaviorData", "DisableBeatSaviorUpload", value);
            }
        }

        [UIValue("result")]
        public string Result
        {
            get
            {
                //var dataRead = new DataRecorderRead();
                //dataRead.ScoreRead(_selectedBeatmap.level.levelID);
                return "";
            }
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }
        public void Initialize()
        {
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
