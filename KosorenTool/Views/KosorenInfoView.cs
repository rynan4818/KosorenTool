using HMUI;
using KosorenTool.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace KosorenTool.Views
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
	public class KosorenInfoView : MonoBehaviour
    {
        public GameObject _rootObject;
        public Canvas _canvas;
        public CurvedTextMeshPro _kosorenInfo;

        public static readonly Vector2 CanvasSize = new Vector2(50, 10);
        public static readonly Vector3 Scale = new Vector3(0.01f, 0.01f, 0.01f);
        public static readonly Vector3 LeftPosition = new Vector3(0, 3f, 4.5f);
        public static readonly Vector3 LeftRotation = new Vector3(0, 0, 0);

        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        public void Awake()
        {
            this._rootObject = new GameObject("Kosoren Info Canvas", typeof(Canvas), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var sizeFitter = this._rootObject.GetComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            this._canvas = this._rootObject.GetComponent<Canvas>();
            this._canvas.sortingOrder = 3;
            this._canvas.renderMode = RenderMode.WorldSpace;
            var rectTransform = this._canvas.transform as RectTransform;
            rectTransform.sizeDelta = CanvasSize;
            this._rootObject.transform.position = LeftPosition + new Vector3(PluginConfig.Instance.InfoXoffset, PluginConfig.Instance.InfoYoffset, PluginConfig.Instance.InfoZoffset);
            this._rootObject.transform.eulerAngles = LeftRotation;
            this._rootObject.transform.localScale = Scale;
            this._kosorenInfo = this.CreateText(this._canvas.transform as RectTransform, string.Empty, new Vector2(10, 31));
            rectTransform = this._kosorenInfo.transform as RectTransform;
            rectTransform.SetParent(this._canvas.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
            this._kosorenInfo.fontSize = PluginConfig.Instance.ViewFontSize;
            this._kosorenInfo.color = Color.red;
            this._kosorenInfo.text = "KOSOREN Enabled!";
            this._rootObject.SetActive(PluginConfig.Instance.DisableSubmission);
        }

        public void KosorenInfoChange(bool value)
        {
            this._rootObject.SetActive(value);
        }

        public CurvedTextMeshPro CreateText(RectTransform parent, string text, Vector2 anchoredPosition)
        {
            return this.CreateText(parent, text, anchoredPosition, new Vector2(0, 0));
        }

        public CurvedTextMeshPro CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var gameObj = new GameObject("CustomUIText");
            gameObj.SetActive(false);

            var textMesh = gameObj.AddComponent<CurvedTextMeshPro>();
            textMesh.rectTransform.SetParent(parent, false);
            textMesh.text = text;
            textMesh.fontSize = 4;
            textMesh.overrideColorTags = true;
            textMesh.color = Color.white;

            textMesh.rectTransform.anchorMin = new Vector2(0f, 0f);
            textMesh.rectTransform.anchorMax = new Vector2(0f, 0f);
            textMesh.rectTransform.sizeDelta = sizeDelta;
            textMesh.rectTransform.anchoredPosition = anchoredPosition;

            gameObj.SetActive(true);
            return textMesh;
        }
    }
}
