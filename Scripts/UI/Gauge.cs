using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    [Header("ゲージが追従するオブジェクト")]
    [SerializeField] GameObject GameObj;
    [SerializeField] Vector3 offset = Vector3.zero;

    public float gaugeValue = 1.0f;

    Image gauge;
    Camera cam;


    void Awake()
    {
        gauge = GetComponent<Image>();
        cam = Camera.main;
        offset = gameObject.GetComponent<RectTransform>().localPosition;
    }

    void Update()
    {
        Vector3 position = GameObj.transform.position;

        // プレイヤーのスクリーン座標（2D）を取得
        Vector3 screen = cam.WorldToScreenPoint(position);
        screen += offset;

        // ゲージの座標を設定
        Vector3 trans = gauge.gameObject.transform.position;
        trans.x = screen.x;
        trans.y = screen.y;
        gauge.gameObject.transform.position = trans;

        if (gaugeValue > 0)
        {
            gauge.fillAmount = gaugeValue;
        }
    }
}