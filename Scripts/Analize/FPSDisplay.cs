using UnityEngine;
/// <summary>
/// インフォメーション簡易表示
/// スクリプトを空のGameObjectにアタッチ
/// </summary>
/// 
public class FPSDisplay : MonoBehaviour
{
    [SerializeField] Vector2 startPosition = new Vector2(10, 10);
    [SerializeField] int fontSize = 24;

    float deltaTime = 0.0f;
    EasyInfoGUI easyInfoGUI = new EasyInfoGUI();

    private void Awake()
    {
        easyInfoGUI.startPosition = startPosition;
        easyInfoGUI.fontSize = fontSize;
    }


    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        
        easyInfoGUI.message = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        easyInfoGUI.Show();
    }
}

