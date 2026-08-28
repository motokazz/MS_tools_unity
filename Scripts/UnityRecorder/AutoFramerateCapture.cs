using UnityEngine;
/// <summary>
/// 録画開始時に Time.captureFramerate を設定し、録画終了時にリセットする
/// UnityRecorderの録画フレームレートが安定しないときの対策スクリプト
/// ～使い方～
/// このスクリプトを空の GameObject にアタッチ。
/// 録画時にこの GameObject を アクティブにする。
/// 録画終了後に 非アクティブにする（または自動で終了させる）。
/// </summary>
public class AutoFramerateCapture : MonoBehaviour
{
    public int captureFramerate = 60;

    void OnEnable()
    {
        Debug.Log("Setting Time.captureFramerate to " + captureFramerate);
        Time.captureFramerate = captureFramerate;
    }

    void OnDisable()
    {
        Debug.Log("Resetting Time.captureFramerate to 0");
        Time.captureFramerate = 0;
    }
}
