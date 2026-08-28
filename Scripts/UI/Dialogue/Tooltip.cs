using UnityEngine;
using TMPro;
using System.Collections;

public class Tooltip : MonoBehaviour
{
    public GameObject tooltipUI;
    public TextMeshProUGUI tooltipText;

    public Vector2 offset = new Vector2(20, 20);


    RectTransform tooltipUIRect;
    Coroutine coroutine;


    void Start()
    {
        tooltipUIRect = tooltipUI.GetComponent<RectTransform>();
        tooltipUI.SetActive(false);
    }


    public void ShowTooltip(string text)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = StartCoroutine(ShowToolTipC(text));
    }

    public void HideTooltip()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        StartCoroutine(HideTooltipC());
        
    }

    IEnumerator ShowToolTipC(string text)
    {
        yield return null;
        yield return new WaitForSeconds(1);
        tooltipText.text = text;
        tooltipUI.SetActive(true);
    }

    IEnumerator HideTooltipC()
    {
        yield return new WaitForSeconds(0.1f);
        tooltipUI.SetActive(false);
    }

    void Update()
    {
        if (!tooltipUI.gameObject.activeSelf) return;

        Vector2 anchoredPos = tooltipUIRect.anchoredPosition;
        /*
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)tooltipUIRect.parent,
            Input.mousePosition + (Vector3)offset,
            null,
            out anchoredPos);
        */

        // サイズを取得
        Vector2 size = tooltipUIRect.sizeDelta;
        RectTransform parentRect = tooltipUIRect.parent as RectTransform;

        // 親キャンバス内でクランプ
        Vector2 min = parentRect.rect.min + size * 0.5f;
        Vector2 max = parentRect.rect.max - size * 0.5f;

        anchoredPos.x = Mathf.Clamp(anchoredPos.x, min.x, max.x);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, min.y, max.y);

        tooltipUIRect.anchoredPosition = anchoredPos;
    }

}
