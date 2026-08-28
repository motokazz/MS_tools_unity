using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 吹き出し付きバルーンダイアログ
/// 表示するときShowで呼び出し。
/// 非表示にするときHide。
/// </summary>

public class BalloonDialog : MonoBehaviour
{
    [Header("吹き出し本体Panel。指定無くてもOK")]
    [SerializeField] private RectTransform panel;
    [Header("吹き出し中の文章表示用Text。指定無くてもOK")]
    [SerializeField] private TextMeshProUGUI text;
    [Header("吹き出しの矢印Panel。指定無くてもOK")]
    [SerializeField] private RectTransform arrow;
    [Header("フォント。指定無くてもOK")]
    [SerializeField] private TMP_FontAsset font;

    [Header("フォントサイズ")]
    [SerializeField] private int fontSize = 24;
    [Header("吹き出し内の余白横・縦")]
    [SerializeField] private Vector2 padding = new Vector2(16, 8);
    [Header("画面に収めるための画面端からの余白")]
    [SerializeField] private float marginX = 10f;
    [SerializeField] private float marginY = 10f;

    [SerializeField] float fadeInWait = 1.0f;
    [SerializeField] float fadeOutWait = 1.0f;


    private Camera cam;
    Coroutine coroutine;


    void Awake()
    {
        cam = Camera.main;
        if (panel == null)
            CreateUI();
    }

    void CreateUI()
    {
        // Canvas直下に生成
        var canvas = GetComponentInParent<Canvas>();
        if (!canvas) canvas = FindAnyObjectByType<Canvas>();

        //吹き出し本体
        GameObject root = new GameObject("BalloonHelpPanel", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        panel = root.GetComponent<RectTransform>();
        panel.pivot = new Vector2(0.5f, 0f);
        panel.GetComponent<Image>().color = new Color(1, 1, 1, 0.9f);
        panel.GetComponent<Image>().raycastTarget = false;

        //テキスト
        GameObject textObj = new GameObject("TextX", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(panel, false);
        text = textObj.GetComponent<TextMeshProUGUI>();

        if (font != null)
        {
            text.font = font;
        }

        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Midline;
        text.color = Color.black;
        text.textWrappingMode = TextWrappingModes.PreserveWhitespace;
        text.margin = new Vector4(8, 4, 8, 4);

        //矢印
        GameObject arrowObj = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
        arrowObj.transform.SetParent(panel, false);
        arrow = arrowObj.GetComponent<RectTransform>();
        arrow.sizeDelta = new Vector2(20, 10);
        arrow.GetComponent<Image>().color = new Color(1, 1, 1, 0.9f);

        root.SetActive(false);

    }

    /// <summary>
    /// 吹き出しを表示
    /// </summary>
    public void ShowWorldPos(string message, Vector3 worldPos)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        panel.position = screenPos + Vector2.up * 40f;
        ShowScreenPos(message, screenPos);
    }

    public void ShowScreenPos(string message, Vector2 screenPos)
    {
        panel.gameObject.SetActive(true);

        text.SetText(message);
        LayoutRebuilder.ForceRebuildLayoutImmediate(text.rectTransform);

        text.ForceMeshUpdate();
        Vector2 textSize = text.GetRenderedValues(false);
        panel.sizeDelta = textSize + padding;

        //
        RectTransform canvasRect = panel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        //ターゲットに対してどこに出すか
        Vector2 screenSize = canvasRect.sizeDelta;
        Vector2 tempPos;
        float shift = 200f;
        tempPos.x = screenPos.x > (screenSize.x * 0.5f) ? -shift : shift;
        tempPos.y = screenPos.y > (screenSize.y * 0.5f) ? -shift*0.5f : shift*0.5f;
        panel.position = screenPos + tempPos;

        // 画面からはみ出ないように補正
        Vector2 panelSize = panel.sizeDelta;
        Vector2 pos = panel.anchoredPosition;

        Vector2 margin = new Vector2(marginX, marginY);
        Vector2 min = canvasRect.rect.min + Vector2.one * margin;
        Vector2 max = canvasRect.rect.max - panelSize / 2f - Vector2.one * margin;
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        panel.anchoredPosition = pos;

        // 矢印の位置を調整
        arrow.anchoredPosition = new Vector2(0, -panel.sizeDelta.y / 2);
    }

    public void Show(string message, Vector2 pos)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(ShowC(message, pos));
    }
    public void Hide()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(HideC());
    }

    IEnumerator ShowC(string message,Vector2 pos)
    {
        yield return null;
        yield return new WaitForSeconds(fadeInWait);
        ShowScreenPos(message, pos);
    }

    IEnumerator HideC()
    {
        yield return null;
        yield return new WaitForSeconds(fadeOutWait);
        panel.gameObject.SetActive(false);
    }
}
