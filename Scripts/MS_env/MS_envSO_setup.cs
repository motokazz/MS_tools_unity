
using UnityEngine;
using UnityEngine.Rendering;


public class MS_envSO_setup : MonoBehaviour
{
	public MS_envSO mS_EnvSO_current;
	Volume volume;

    private void Start()
    {
        
    }

    public void Store(MS_envSO mS_EnvSO)
    {
		mS_EnvSO_current.universalRendererData = mS_EnvSO.universalRendererData;

		volume = Component.FindObjectsOfType<Volume>()[0];
        if (volume)
        {
			mS_EnvSO_current.volumeProfile = volume.profile;
			mS_EnvSO_current.volumeWeight = volume.weight;
		}
		mS_EnvSO_current.ambientOcculusion = mS_EnvSO.universalRendererData.rendererFeatures[0].isActive;
		mS_EnvSO_current.customPostEffect1 = mS_EnvSO.universalRendererData.rendererFeatures[1].isActive;
		mS_EnvSO_current.customPostEffect2 = mS_EnvSO.universalRendererData.rendererFeatures[2].isActive;

		mS_EnvSO_current.skyboxMaterial = RenderSettings.skybox;

		mS_EnvSO_current.realtimeShadowColor = RenderSettings.subtractiveShadowColor;

		mS_EnvSO_current.ambientMode = RenderSettings.ambientMode;

		mS_EnvSO_current.ambientSkyColor = RenderSettings.ambientSkyColor;
		mS_EnvSO_current.ambientEquaterColor = RenderSettings.ambientEquatorColor;
		mS_EnvSO_current.ambientGroundColor = RenderSettings.ambientGroundColor;

		mS_EnvSO_current.reflectionIntensityMultiplyer = RenderSettings.reflectionIntensity;

		//RenderSetup Fog
		mS_EnvSO_current.fog = RenderSettings.fog;
		mS_EnvSO_current.fogMode = RenderSettings.fogMode;
		mS_EnvSO_current.fogColor = RenderSettings.fogColor;
		mS_EnvSO_current.fogDensity = RenderSettings.fogDensity;

	}

	public void Recall(MS_envSO mS_EnvSO)
    {

		volume = Component.FindObjectsOfType<Volume>()[0];
		if (volume)
		{
			volume.profile = mS_EnvSO.volumeProfile;
			volume.weight = mS_EnvSO.volumeWeight;
		}

		//MS_envSOからカスタムポストエフェクトをセット
		mS_EnvSO.universalRendererData.rendererFeatures[0].SetActive(mS_EnvSO.ambientOcculusion);
		mS_EnvSO.universalRendererData.rendererFeatures[1].SetActive(mS_EnvSO.customPostEffect1);
		mS_EnvSO.universalRendererData.rendererFeatures[2].SetActive(mS_EnvSO.customPostEffect2);

		//RenderSetup Environment
		RenderSettings.skybox = mS_EnvSO.skyboxMaterial;
		RenderSettings.subtractiveShadowColor = mS_EnvSO.realtimeShadowColor;

		RenderSettings.ambientMode = mS_EnvSO.ambientMode;

		RenderSettings.ambientSkyColor = mS_EnvSO.ambientSkyColor;
		RenderSettings.ambientEquatorColor = mS_EnvSO.ambientEquaterColor;
		RenderSettings.ambientGroundColor = mS_EnvSO.ambientGroundColor;

		RenderSettings.reflectionIntensity = mS_EnvSO.reflectionIntensityMultiplyer;

		//RenderSetup Fog
		RenderSettings.fog = mS_EnvSO.fog;
		RenderSettings.fogMode = mS_EnvSO.fogMode;
		RenderSettings.fogColor = mS_EnvSO.fogColor;
		RenderSettings.fogDensity = mS_EnvSO.fogDensity;
	}

}
