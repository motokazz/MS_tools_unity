
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// GGGレンダリング設定を作業中に呼び出すためのツール
/// </summary>
/// 

public class MS_environmentModifier_path
{
    public string path;
}

[FilePath("ProjectSettings/MS_environmentModifier_setting.asset", FilePathAttribute.Location.ProjectFolder)]
public class MS_environmentModifier_setting : ScriptableSingleton<MS_environmentModifier_setting>
{
    [SerializeField] public string path;
    public MS_environmentModifier_path empath;
    public void SaveSettings()
    {
        Save(true); // 保存
    }
}

public class MS_environmentModifier : EditorWindow
{

	//メインのMS_EnvSO
	public MS_envSO mS_EnvSO;

	//グローバルボリューム格納
	Volume[] volumes;

	//実行時にも設定を適用する
	bool isEditableOnPlay;

	[MenuItem("MS_Tools/MS_environmentModifier")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_environmentModifier));
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("MS_environmentModifier");
        EditorGUILayout.LabelField("※起動したらMS_envSOを直接弄りましょう。");
        mS_EnvSO = (MS_envSO)EditorGUILayout.ObjectField(mS_EnvSO, typeof(MS_envSO), false);
        if (mS_EnvSO == null)
        {
            return;
        }

        //実行中にレンダー適用するかどうか
        isEditableOnPlay = (bool)EditorGUILayout.ToggleLeft("isEditableOnPlay", isEditableOnPlay);
        if (!EditorApplication.isPlaying || isEditableOnPlay )
        {
            if (EditorGUI.EndChangeCheck())
            {
                volumes = Component.FindObjectsByType<Volume>(FindObjectsSortMode.None);
                if (volumes.Length > 0)
                {
                    SetVolume(volumes[0]);
                }

                SetRenderFeatures();
                SetRenderSettings();

                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
    }

    private void OnEnable()
    {
        var settings = MS_environmentModifier_setting.instance;

        if (settings != null)
        {
            mS_EnvSO = (MS_envSO)AssetDatabase.LoadAssetAtPath<MS_envSO>(settings.path);
            //Debug.Log(mS_EnvSO.description);
        }
	}

    private void OnDisable()
    {
        var settings = MS_environmentModifier_setting.instance;
        Debug.Log(AssetDatabase.GetAssetPath(mS_EnvSO));
        settings.path = (string)AssetDatabase.GetAssetPath(mS_EnvSO);
        settings.SaveSettings();

    }

    /// <summary>
    /// GlovalVolumeの設定
    /// </summary>
    /// <param name="volume"></param>
	private void SetVolume(Volume volume)
	{
        if (volume != null)
        {
            volume.profile = mS_EnvSO.volumeProfile;
            volume.weight = mS_EnvSO.volumeWeight;
        }
	}

    /// <summary>
    /// RenderFeatures設定
    /// </summary>
	private void SetRenderFeatures()
	{
        if(mS_EnvSO.universalRendererData != null)
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
	}

    /// <summary>
    /// RenderSettings設定
    /// </summary>
	private void SetRenderSettings()
	{
		//MS_envSOからレンダリングをセットアップ
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

		DynamicGI.UpdateEnvironment();
	}


}
