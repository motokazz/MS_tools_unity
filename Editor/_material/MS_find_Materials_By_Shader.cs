using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMaterialsByShader : EditorWindow
{
    private Shader targetShader;
    private Vector2 scrollPosition;
    private List<Material> matchedMaterials = new List<Material>();

    [MenuItem("MS_Tools/Material/Find Materials By Shader")]
    public static void ShowWindow()
    {
        GetWindow<FindMaterialsByShader>("Find Materials By Shader");
    }

    void OnGUI()
    {
        GUILayout.Label("Search Materials Using Shader", EditorStyles.boldLabel);

        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);

        if (GUILayout.Button("Find Materials"))
        {
            FindMaterials();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Found {matchedMaterials.Count} material(s):", EditorStyles.label);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var mat in matchedMaterials)
        {
            if (GUILayout.Button(mat.name, EditorStyles.objectField))
            {
                EditorGUIUtility.PingObject(mat);
                Selection.activeObject = mat;
            }
        }

        GUILayout.EndScrollView();

        if (matchedMaterials.Count > 0)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Select All in Project"))
            {
                Selection.objects = matchedMaterials.ToArray();
            }
        }
    }

    private void FindMaterials()
    {
        matchedMaterials.Clear();

        if (targetShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Shader.", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader == targetShader)
            {
                matchedMaterials.Add(mat);
            }
        }
    }
}
