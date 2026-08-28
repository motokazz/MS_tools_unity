using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MS_tools;

public class MS_materialTool : EditorWindow
{
    //設定読み込み
    private Shader targetShader; // 差し替え元シェーダー
    private Shader newShader; // 差し替えるシェーダー
    
    private Material[] selectedMaterials; // 選択されたマテリアル

    [MenuItem("MS_Tools/Material/MS_materialTool")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_materialTool));
    }

    void OnGUI()
    {

        EditorGUILayout.LabelField("GameMain");

        //Tool
        EditorGUILayout.LabelField("tools");

        GUILayout.Label("target Shader Changer", EditorStyles.boldLabel);
        targetShader = (Shader)EditorGUILayout.ObjectField("target Shader", targetShader, typeof(Shader), false);

        GUILayout.Label("Shader Changer", EditorStyles.boldLabel);
        newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);


        // シェーダーを選択したマテリアルに適用
        if (selectedMaterials != null && selectedMaterials.Length > 0)
        {
            if (GUILayout.Button("Apply Shader to Selected Materials"))
            {
                ApplyShaderToSelectedMaterials();
            }

            GUILayout.Label("Selected Materials:");
            foreach (var material in selectedMaterials)
            {
                GUILayout.Label(material.name);
            }
        }
        else
        {
            GUILayout.Label("No materials selected.");
        }
    }



    // 選択されたオブジェクトのマテリアルを取得
    private void LoadSelectedMaterials()
    {
        var objects = Selection.objects;
        var materials = new System.Collections.Generic.List<Material>();

        foreach (var obj in objects)
        {
            if (obj is Material)
            {
                materials.Add(obj as Material);
            }
            else if (obj is GameObject)
            {
                var renderer = (obj as GameObject).GetComponent<Renderer>();
                if (renderer != null)
                {
                    materials.AddRange(renderer.sharedMaterials);
                }
            }
        }

        selectedMaterials = materials.ToArray();
    }

    // 選択されたマテリアルに新しいシェーダーを適用
    private void ApplyShaderToSelectedMaterials()
    {
        LoadSelectedMaterials();

        if (newShader == null)
        {
            Debug.LogWarning("Please select a shader before applying.");
            return;
        }

        foreach (var material in selectedMaterials)
        {
            Undo.RecordObject(material, "Change Shader");

            if (material.shader == targetShader)
            {
                SwapShader(material);
            }

            EditorUtility.SetDirty(material); // 変更を保存
        }

        Debug.Log("Shader applied to selected materials.");
    }


    void SwapShader(Material material)
    {

        // 既存のパラメータを保存
        Texture mainTexture = material.GetTexture("_BaseColor_Texture");
        Color color = material.GetColor("_MainAlbedoTint");
        float metallic = material.GetFloat("_Metallic");
        float smoothness = material.GetFloat("_Glossiness");
        Texture normalTexture = material.GetTexture("_Normal_Texture");
        Texture emissiveTexture = material.GetTexture("_EmissiveMask");
        Color emissiveColor = material.GetColor("_EmissiveColor");
        // 新しいシェーダーに変更
        material.shader = newShader;

        // 保存したパラメータを再設定
        material.SetTexture("_BaseMap", mainTexture);
        material.SetTexture("_EmissionMap", emissiveTexture);
        material.SetColor("_Color", color);
        material.SetFloat("_Metallic", 0);
        material.SetFloat("_Smoothness", 0);
        material.SetTexture("_BumpMap", normalTexture);
        material.SetColor("_EmissionColor", emissiveColor);

    }
}
