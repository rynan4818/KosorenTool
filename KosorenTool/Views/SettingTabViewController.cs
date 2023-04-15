using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using KosorenTool.Configuration;
using System;
using System.Collections.Generic;
using Zenject;

namespace KosorenTool.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable
    {
        public string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

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

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }
        public void Initialize()
        {
            GameplaySetup.instance.AddTab(Plugin.Name, this.ResourceName, this);
        }
    }
}
