
using UnityEngine;
using UnityEngine.Rendering;//volumeを使うのに、この行が必要です。
using UnityEngine.Rendering.Universal;//DepthOfFieldを使うのに、この行が必要です。

// 再生中じゃなくてもスクリプトを適用する
[ExecuteAlways]

// TimelineでパラメータをいじるにはAnimatorが付いている必要があるので、
// このスクリプトを付けると自動でAnimatorがアタッチされるように以下を追記します。
[RequireComponent(typeof(Animator))]

public class GGG_GlovalVolume_Controller : MonoBehaviour
{
    [SerializeField]
    VolumeProfile volumeProfile;//Volumeコンポーネントへの参照
    VolumeProfile volumeProfileStored;//VolumeProfileの取得・破棄の監視用

    [Header("Vignette")]
    Vignette vignette;
    [SerializeField] Color vignetteColor = new Color(0, 0, 0);
    Color vignetteColorStored = new Color(0, 0, 0);
    [SerializeField] float vignetteIntensity = 0.5f;
    float vignetteIntensityStored;

    [Header("ChromaticAberration")]
    ChromaticAberration chromaticAberration;
    [SerializeField] float chromaticAbarrationIntensity = 0;
    float chromaticAbarrationIntensityStored;

    [Header("LensDistortion")]
    LensDistortion lensDistortion;
    [SerializeField] float lensDistortionIntensity = 0;
    float lensDistortionIntensityStored;
    [SerializeField] float lensDistortionScale = 0;
    float lensDistortionScaleStored;

    //テンプレート
    [SerializeField] int template = 0;
    int templateStored;
    [SerializeField] float smoothFactor = 0.1f;



    //インスペクタでスクリプトが読み込まれたor値が変わった時に実行される
    void OnValidate()
    {
        //volumeProfileがまだ設定されていないのであれば戻る
        if (volumeProfile == null && volumeProfileStored == null) return;

        //volumeProfileが変更されていない場合は戻る。
        if (volumeProfileStored == volumeProfile) return;

        //volumeProfileが削除された場合、メンバとして格納しているVolumeCompornentへの参照を破棄して戻る。
        if (volumeProfile == null && volumeProfileStored != null)
        {
            vignette = null;
            chromaticAberration = null;
            lensDistortion = null;
            return;
        }

        // コンポーネントを探して格納する
        foreach (var item in volumeProfile.components)
        {
            if (item as Vignette)
            {
                vignette = item as Vignette;
            }

            if (item as ChromaticAberration)
            {
                chromaticAberration = item as ChromaticAberration;
            }

            if (item as LensDistortion)
            {
                lensDistortion = item as LensDistortion;
            }
        }

    }

    void Update()
    {
        //テンプレートの更新
        if (template != templateStored)
        {
            ChangeTemplate(template);
            templateStored = template;
        }

        VignetteControl();
        ChromaticAberrationControl();
        LensDistortionControl();


        //volumeProfileの更新（OnValidate()でやればいい気もする）
        volumeProfileStored = volumeProfile;
    }

    /// <summary>
    /// 状態毎にポストエフェクトの値を設定
    /// </summary>
    /// <param name="templateNo"></param>
    void ChangeTemplate(int templateNo)
    {
        if (templateNo == 0)
        {
            vignetteIntensity = 0f;
        }
        else if (templateNo == 1)//半牛鬼
        {
            vignetteIntensity = 0.5f;
        }
        else if (templateNo == 2)//ダウン
        {
            vignetteIntensity = 1f;
        }
        else
        {
            vignetteIntensity = 0f;
        }

    }




    void VignetteControl()
    {
        if (vignette) // 参照が格納されていれば値を操作する。
        {
            //VolumeCompornen上の現在値とバックアップ値が不一致の場合（スクリプトアタッチ時など）
            if (vignette.color.value != vignetteColorStored)
            {
                //バックアップ値をVolumeCompornen上の現在値で更新
                vignetteColorStored = vignette.color.value;
            }
            //バックアップ値とスライダー値が不一致の場合（Animatorが更新した時など）
            else if (vignetteColor != vignetteColorStored)
            {
                //バックアップ値をスライダー値で更新
                vignetteColorStored = vignetteColor;
            }

            if (vignette.intensity.value != vignetteIntensityStored)
            {
                //vignetteIntensityStored = Mathf.Lerp(vignetteIntensityStored, vignette.intensity.value, smoothFactor);
                vignetteIntensityStored = vignette.intensity.value;
            }
            else if (vignetteIntensity != vignetteIntensityStored)
            {
                vignetteIntensityStored = Mathf.Lerp(vignetteIntensityStored, vignetteIntensity, smoothFactor);
            }
        }
        //スライダー値をバックアップ値で更新
        vignette.color.value = vignetteColorStored;
        vignette.intensity.value = vignetteIntensityStored;
    }

    void ChromaticAberrationControl()
    {
        if (chromaticAberration)
        {
            if (chromaticAberration.intensity.value != chromaticAbarrationIntensityStored)
            {
                chromaticAbarrationIntensityStored = chromaticAberration.intensity.value;
            }
            else if (chromaticAbarrationIntensity != chromaticAbarrationIntensityStored)
            {
                chromaticAbarrationIntensityStored = chromaticAbarrationIntensity;
            }
        }
        chromaticAberration.intensity.value = chromaticAbarrationIntensityStored;
    }

    void LensDistortionControl()
    {
        if (lensDistortion)
        {
            //intensity
            if (lensDistortion.intensity.value != lensDistortionIntensityStored)
            {
                lensDistortionIntensityStored = lensDistortion.intensity.value;
            }
            else if (lensDistortionIntensity != lensDistortionIntensityStored)
            {
                lensDistortionIntensityStored = lensDistortionIntensity;
            }
            
            //scale
            if (lensDistortion.scale.value != lensDistortionScaleStored)
            {
                lensDistortionScaleStored = lensDistortion.scale.value;
            }
            else if (lensDistortionScale != lensDistortionScaleStored)
            {
                lensDistortionScaleStored = lensDistortionScale;
            }
            
        }
        lensDistortion.intensity.value = lensDistortionIntensityStored;
        lensDistortion.scale.value = lensDistortionScaleStored;
    }

}

