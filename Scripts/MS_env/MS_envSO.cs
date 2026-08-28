using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[CreateAssetMenu(fileName = "MS_envSO", menuName = "MS_tools/MS_envSO")]
public class MS_envSO : ScriptableObject
{
    [Header("CustomPostEffect")]
    public UniversalRendererData universalRendererData;
    public bool ambientOcculusion = false;
    
    public bool customPostEffect1 = false;
    public RenderObjects renderObjects1;
    public Material overrideMaterial1;

    public bool customPostEffect2 = false;
    public RenderObjects renderObjects2;
    public Material overrideMaterial2;

    [Header("GlobalVolume")]
    public VolumeProfile volumeProfile;
   [SerializeField,Range(0,1)] public float volumeWeight = 1;

    /// 環境光の定義
    /// </summary>
    /// 
    [Header("Environment")]
    public Material skyboxMaterial;
    public Color realtimeShadowColor = new Color(0.42f,0.478f,0.627f,1.0f);

    [Header("EnvironmentLighting")]
    public AmbientMode ambientMode = AmbientMode.Skybox;
    [ColorUsage(false, true)] public Color ambientSkyColor = new Color(0.212f,0.227f,0.259f,0.0f);
    [ColorUsage(false, true)] public Color ambientEquaterColor = new Color(0.114f,0.125f,0.133f,0.0f);
    [ColorUsage(false, true)] public Color ambientGroundColor = new Color(0.047f,0.043f,0.035f,0.0f);

    [Header("EnvironmentReflections")]
    [SerializeField, Range(0, 100)] public float reflectionIntensityMultiplyer = 1.0f;

    [Header("Fog")]
    public bool fog = false;
    public Color fogColor = new Color(0.5f,0.5f,0.5f,1.0f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;

    [Header("Description")]
    public string description;

}