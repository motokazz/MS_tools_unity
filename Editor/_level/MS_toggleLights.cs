using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class MS_toggleLights : EditorWindow
{
    GameObject[] gameObjectsAll;
    
    public class SceneLight
    {
        public Light light;
        public LightType lightType;
        public bool activate;
    }

    List<SceneLight> sceneLights = new List<SceneLight>();

    [MenuItem("MS_Tools/Level/MS_toggleLights")]


    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_toggleLights));
    }
    void OnGUI()
    {

        EditorGUILayout.LabelField("MS_toggleLights");

        if (GUILayout.Button("PointLightON"))
        {
            ToggleAdditionalLights();
        }
    }
    void OnEnable()
    {
        gameObjectsAll = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach(GameObject go in gameObjectsAll)
        {
            if (go.GetComponent<Light>())
            {
                Light light = go.GetComponent<Light>();
                if (light.type == LightType.Point)
                {
                    SceneLight sl = new SceneLight();
                    sl.light = light;
                    sl.lightType = LightType.Point;
                    sl.activate = light.enabled;
                    sceneLights.Add(sl);
                }
            }
        }
    }
    private void OnDisable()
    {
        foreach(SceneLight sl in sceneLights)
        {
            sl.light.enabled = sl.activate;
        }
    }


    void ToggleAdditionalLights()
    {
        foreach(SceneLight sl in sceneLights)
        {
            sl.light.enabled = true;
        }
    }

}
