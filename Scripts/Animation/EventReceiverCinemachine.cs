using UnityEngine;
using Unity.Cinemachine;
/// <summary>
/// アニメーションイベントOnRightFoot、OnLeftFootを受けてカメラを揺らす。
/// </summary>

public class EventReceiverCinemachine:MonoBehaviour
{
    [SerializeField] CinemachineCamera cam;
    [SerializeField] NoiseSettings noiseSettings;
    [SerializeField] float amplitudeGain = 1.0f;
    [SerializeField] float frequencyGain = 1.0f;
    [SerializeField] float duration = 1.0f;
    [SerializeField] float distanceMax = 10f;
    CinemachineBasicMultiChannelPerlin channelPerlin;
    float shakeTimer = 0.0f;
    


    private void Awake()
    {

        channelPerlin = cam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (channelPerlin == null)
        {
            channelPerlin = cam.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
        }

        //初期化
        channelPerlin.NoiseProfile = noiseSettings;
        channelPerlin.AmplitudeGain = 0;
        channelPerlin.FrequencyGain = 0;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            // タイマーを減少させる
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0)
            {
                StopShake(); // 振動停止
            }
        }
    }


    public void OnRightFoot()
    {
        ShakeCamera();
    }

    public void OnLeftFoot() {
        ShakeCamera();
    }

    public void ShakeCamera()
    {
        float distance = (transform.position - cam.transform.position).magnitude;
        float mag = 1 - Mathf.Clamp(distance / distanceMax, 0, 1);
        if (channelPerlin == null) return;

        // 振動の強度を設定
        channelPerlin.AmplitudeGain = amplitudeGain*mag;
        channelPerlin.FrequencyGain = frequencyGain; // 振動速度は固定（必要なら変更可）

        // タイマーを設定
        shakeTimer = duration;
    }

    public void StopShake()
    {
        if (channelPerlin == null) return;

        // 振動をリセット
        channelPerlin.AmplitudeGain = 0f;
        channelPerlin.FrequencyGain = 0f;
    }
}
