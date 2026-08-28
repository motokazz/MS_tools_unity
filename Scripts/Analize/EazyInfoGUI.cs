using UnityEngine;
/// <summary>
/// デバッグ表示用の簡易スクリーンメッセージ表示
/// ～使い方～
/// 各スクリプト内でEasyInfoUIを定義して各プロパティ設定
/// void OnGUI等でShow()を呼び出してmessageを表示
/// </summary>
/// 
public class EasyInfoGUI : MonoBehaviour
{

    public string message;
    public Vector2 startPosition = new Vector2(10,10);
    public int fontSize = 24;

    public void Show()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(startPosition.x, startPosition.y, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;

        GUI.Label(rect, message, style);
    }
}
