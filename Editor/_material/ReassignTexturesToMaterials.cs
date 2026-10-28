using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

public class ReassignTexturesToMaterials : EditorWindow
{
    private DefaultAsset targetFolder;
    private bool usePartialMatch = false;

    [MenuItem("MS_Tools/Materials/Reassign Textures")]
    public static void ShowWindow()
    {
        GetWindow<ReassignTexturesToMaterials>("Reassign Textures");
    }

    void OnGUI()
    {
        GUILayout.Label("Reassign Textures to Selected Materials", EditorStyles.boldLabel);

        // 検索対象のフォルダをドラッグ＆ドロップで指定
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);

        // 部分一致オプション
        usePartialMatch = EditorGUILayout.Toggle("Allow Partial Match", usePartialMatch);

        GUILayout.Space(10);

        if (GUILayout.Button("Reassign Textures"))
        {
            ReassignTextures();
        }
    }

    private void ReassignTextures()
    {
        if (targetFolder == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target folder.", "OK");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Selected object is not a valid folder.", "OK");
            return;
        }

        // 選択中のマテリアルを取得（フォルダ選択時も中のマテリアルを抽出）
        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Notice", "Please select materials or folders containing materials in the Project window.", "OK");
            return;
        }

        // 指定フォルダ内の全テクスチャをキャッシュ
        string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
        List<Texture> availableTextures = new List<Texture>();
        foreach (string guid in texGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex != null)
            {
                availableTextures.Add(tex);
            }
        }

        if (availableTextures.Count == 0)
        {
            EditorUtility.DisplayDialog("Notice", "No textures found in the selected folder.", "OK");
            return;
        }

        int updatedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            Material mat = obj as Material;
            if (mat == null) continue;

            bool isModified = false;

            // マテリアルが持つすべてのテクスチャプロパティ（_MainTex, _BumpMapなど）を取得
            string[] textureProperties = mat.GetTexturePropertyNames();

            foreach (string propName in textureProperties)
            {
                Texture currentTex = mat.GetTexture(propName);
                if (currentTex == null) continue; // テクスチャが未割り当ての場合はスキップ

                Texture matchedTex = FindMatchingTexture(currentTex.name, availableTextures);

                if (matchedTex != null && matchedTex != currentTex)
                {
                    mat.SetTexture(propName, matchedTex);
                    isModified = true;
                }
            }

            if (isModified)
            {
                EditorUtility.SetDirty(mat);
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Complete", $"Successfully updated textures for {updatedCount} material(s).", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Complete", "No materials were updated. (No matching textures found or already assigned)", "OK");
        }
    }

    private Texture FindMatchingTexture(string originalName, List<Texture> availableTextures)
    {
        // 1. 完全一致を探す (大文字・小文字を区別しない)
        foreach (Texture tex in availableTextures)
        {
            if (string.Equals(tex.name, originalName, StringComparison.OrdinalIgnoreCase))
            {
                return tex;
            }
        }

        // 2. 部分一致を探す (オプションが有効な場合)
        if (usePartialMatch)
        {
            foreach (Texture tex in availableTextures)
            {
                // 元のテクスチャ名が新しいテクスチャ名に含まれている、またはその逆
                if (tex.name.IndexOf(originalName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    originalName.IndexOf(tex.name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return tex;
                }
            }
        }

        return null; // 一致なし
    }
}