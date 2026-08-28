using UnityEngine;
using UnityEngine.VFX;
public class VFXToLightSync : MonoBehaviour
{
    public Light light;
    public string lightParameter;
    public AnimationCurve lightAnimCurve; // インスペクターでカーブを編集
    public float multiplier = 1.0f;     // 強度の倍率
    public float duration = 1.0f;        // アニメーション時間

    private float timer = 0f;

    void Update()
    {
        if (light == null) return;

        // 0〜1の範囲で時間を進める
        timer += Time.deltaTime / duration;

        // カーブから値を読み取る (0.0 〜 1.0)
        float curveValue = lightAnimCurve.Evaluate(timer % 1.0f);

        // ライトの強度に適用
        light.intensity = curveValue * multiplier;
    }
}