using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// RayがHitしたオブジェクトにHelpMessageコンポーネントがあったら、Messageの内容をBaloonDialogにして表示する
/// </summary>

public class ShowMoseOverHelpMessage: MonoBehaviour
{
    [SerializeField] private BalloonDialog balloonDialog;
    [SerializeField] private GraphicRaycaster m_Raycaster;
    [SerializeField] private EventSystem m_EventSystem;

    void Update()
    {
        // RaycastがUIを透過しないようにするチェック
        if (EventSystem.current.IsPointerOverGameObject())
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                var helpMessage = selected.GetComponent<HelpMessage>();
                if (helpMessage != null)
                {
                    var pos = GetRaycastPosition();
                    balloonDialog.ShowScreenPos(helpMessage.message, pos);
                }
            }
        }
    }

    Vector3 GetRaycastPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
        }
        Vector3 screenPoint = Input.mousePosition;
        // スクリーン座標のZを設定（画面に近づけるために1を代入）
        screenPoint.z = 1.0f;

        // スクリーン座標をワールド座標に変換
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPoint);
        return pos; 
    }
}
