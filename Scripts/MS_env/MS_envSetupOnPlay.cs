using UnityEngine;

public class MS_envSetupOnPlay : MonoBehaviour
{

    [Header("Environment Template")]
    MS_envSO mS_EnvSO;
    [SerializeField] MS_envSO[] mS_EnvSOs;

    //カスタムポストエフェクト現在の設定
    bool ambientOcculusion = false;
    bool customPostEffect1;
    bool customPostEffect2;

    int mS_EnvSOs_current_count = 0;
    int mS_EnvSOs_max_count = 0;

    void Start()
    {

        mS_EnvSOs_max_count = mS_EnvSOs.Length;
        ShaderChange(0);
    }

    public void Restore()
    {
        ShaderChange(0);
    }

    void ShaderChange(int count)
    {
        mS_EnvSOs_current_count += count;
        if (mS_EnvSOs_current_count >= mS_EnvSOs_max_count)
        {
            mS_EnvSOs_current_count = 0;
        }
        else if (mS_EnvSOs_current_count < 0)
        {
            mS_EnvSOs_current_count = mS_EnvSOs.Length - 1;
        }
        mS_EnvSO = mS_EnvSOs[mS_EnvSOs_current_count];
        SetRenderFeatures();
        SetRenderSettings();
    }

    /// <summary>
    /// 終了時：カスタムポストエフェクト原状復帰
    /// </summary>
    private void OnApplicationQuit()
    {
        mS_EnvSO.universalRendererData.rendererFeatures[0].SetActive(ambientOcculusion);
        mS_EnvSO.universalRendererData.rendererFeatures[1].SetActive(customPostEffect1);
        mS_EnvSO.universalRendererData.rendererFeatures[2].SetActive(customPostEffect2);
    }

    private void SetRenderFeatures()
    {
        //MS_envSOからAmbientOcculusionをセット
        mS_EnvSO.universalRendererData.rendererFeatures[0].SetActive(mS_EnvSO.ambientOcculusion);

        //MS_envSOからカスタムポストエフェクト１をセット
        mS_EnvSO.renderObjects1.SetActive(mS_EnvSO.customPostEffect1);
        mS_EnvSO.renderObjects1.settings.overrideMaterial = mS_EnvSO.overrideMaterial1;

        //MS_envSOからカスタムポストエフェクト２をセット
        mS_EnvSO.renderObjects2.SetActive(mS_EnvSO.customPostEffect2);
        mS_EnvSO.renderObjects2.settings.overrideMaterial = mS_EnvSO.overrideMaterial2;
        mS_EnvSO.universalRendererData.SetDirty();

    }

    private void SetRenderSettings()
    {
        //RenderSetup Environment
        RenderSettings.skybox = mS_EnvSO.skyboxMaterial;

        RenderSettings.subtractiveShadowColor = mS_EnvSO.realtimeShadowColor;

        RenderSettings.ambientMode = mS_EnvSO.ambientMode;

        RenderSettings.ambientSkyColor = mS_EnvSO.ambientSkyColor;
        RenderSettings.ambientEquatorColor = mS_EnvSO.ambientEquaterColor;
        RenderSettings.ambientGroundColor = mS_EnvSO.ambientGroundColor;
        if (mS_EnvSO.skyboxMaterial.HasProperty("mainTexture"))
        {
            RenderSettings.customReflectionTexture = mS_EnvSO.skyboxMaterial.mainTexture;
        }
        RenderSettings.reflectionIntensity = mS_EnvSO.reflectionIntensityMultiplyer;

        //RenderSetup Fog
        RenderSettings.fog = mS_EnvSO.fog;
        RenderSettings.fogMode = mS_EnvSO.fogMode;
        RenderSettings.fogColor = mS_EnvSO.fogColor;
        RenderSettings.fogDensity = mS_EnvSO.fogDensity;

        DynamicGI.UpdateEnvironment();
    }
}
