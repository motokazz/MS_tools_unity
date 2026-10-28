using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMaterialsByShader : EditorWindow
{
    private Shader targetShader;
    private Vector2 scrollPosition;
    private List<Material> matchedMaterials = new List<Material>();

    [MenuItem("MS_Tools/Materials/Find Materials By Shader")]
    public static void ShowWindow()
    {
        GetWindow<FindMaterialsByShader>("Find Materials By Shader");
    }

    void OnGUI()
    {
        GUILayout.Label("Search Materials Using Shader", EditorStyles.boldLabel);

        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);

        // 検索ボタンを横に並べる
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Find in Project"))
        {
            FindMaterialsInProject();
        }

        if (GUILayout.Button("Find in Selection"))
        {
            FindMaterialsInSelection();
        }
        GUILayout.EndHorizontal();

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
            // リストの内容に合わせてボタン名を少し汎用的に変更
            if (GUILayout.Button("Select All in List"))
            {
                Selection.objects = matchedMaterials.ToArray();
            }
        }
    }

    // 元の FindMaterials メソッドをプロジェクト用としてリネーム
    private void FindMaterialsInProject()
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

    // 新規追加：選択中のオブジェクトからマテリアルを探す
    private void FindMaterialsInSelection()
    {
        matchedMaterials.Clear();

        if (targetShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Shader.", "OK");
            return;
        }

        // DeepAssets を使うことで、選択したフォルダの中にあるマテリアルも再帰的に取得できます
        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Notice", "Please select materials or folders in the Project window first.", "OK");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            Material mat = obj as Material;
            if (mat != null && mat.shader == targetShader)
            {
                matchedMaterials.Add(mat);
            }
        }
    }
}