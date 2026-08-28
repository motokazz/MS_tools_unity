using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタンで開閉（移動を使って）するUI作成
/// 移動量の数値設定のみで作動する
/// </summary>
/// 

public class UISlideToggle : MonoBehaviour
{
    [SerializeField] RectTransform panel;   // 開閉したいUI
    [SerializeField] Button button;
    [SerializeField] Vector2 hiddenPos;     // 閉じた位置
    [SerializeField] Vector2 shownPos;      // 開いた位置
    [SerializeField] float speed = 10f;
    [SerializeField] bool isOpen = false;

    private Vector2 targetPos;

    void Start()
    {
        button.onClick.AddListener(() => Toggle());

        if (!isOpen)
        {
            panel.anchoredPosition = hiddenPos;
            targetPos = hiddenPos;
        }
    }

    void Update()
    {
        panel.anchoredPosition = Vector2.Lerp(panel.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        targetPos = isOpen ? shownPos : hiddenPos;
    }
}
