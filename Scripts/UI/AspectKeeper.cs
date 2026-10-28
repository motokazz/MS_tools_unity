using UnityEngine;
/// <summary>
/// カメラのアスペクト比を固定するスクリプトを導入する
/// . このスクリプトを、シーン内のメインカメラ（Main Camera）にアタッチします。
/// .インスペクターから Target Width と Target Height に固定したい比率（例: 16 と 9）を入力します。
/// </summary>
/// 
[RequireComponent(typeof(Camera))]
public class AspectKeeper : MonoBehaviour
{
    [SerializeField] private float targetWidth = 16f;  // 目的のアスペクト比（横）
    [SerializeField] private float targetHeight = 9f;  // 目的のアスペクト比（縦）

    private Camera targetCamera;

    void Start()
    {
        targetCamera = GetComponent<Camera>();
        UpdateAspectRatio();
    }

    // ウィンドウサイズが変更されたときのためにUpdateや定期的なタイミングで実行
    void Update()
    {
        UpdateAspectRatio();
    }

    void UpdateAspectRatio()
    {
        // ターゲットとするアスペクト比
        float targetAspect = targetWidth / targetHeight;
        // 現在の画面のアスペクト比
        float windowAspect = (float)Screen.width / Screen.height;
        // スケール値
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = targetCamera.rect;

        if (scaleHeight < 1.0f)
        {
            // 画面が縦に長い場合（横に黒帯を入れる）
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // 画面が横に長い場合（縦に黒帯を入れる）
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        targetCamera.rect = rect;
    }
}
