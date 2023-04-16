using UnityEngine;
using BS_Utils.Gameplay;
using KosorenTool.Configuration;

namespace KosorenTool.Models
{
    public class KosorenToolController : MonoBehaviour
    {
        private void Awake()
        {
                ScoreSubmission.DisableSubmission(Plugin.Name);
        }
    }
}
