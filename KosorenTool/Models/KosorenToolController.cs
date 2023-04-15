using UnityEngine;
using BS_Utils.Gameplay;
using KosorenTool.Configuration;

namespace KosorenTool.Models
{
    public class KosorenToolController : MonoBehaviour
    {
        private void Awake()
        {
            if (PluginConfig.Instance.DisableSubmission)
            {
                ScoreSubmission.DisableSubmission(Plugin.Name);
            }
        }

    }
}
