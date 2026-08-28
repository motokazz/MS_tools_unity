using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.IO;


public class CreateMaterialFromTextures : EditorWindow
{
    [MenuItem("MS_Tools/Material/MS_createMaterialFromTextures")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreateMaterialFromTextures));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("MS_createMaterialFromTextures");
        GUILayout.BeginVertical();


        // SelectSourcePrefabs
        if (GUILayout.Button("CreateURPLitMaterial", GUILayout.Width(200), GUILayout.Height(30)))
        {
            CreateURPLitMaterial();
        }

        GUILayout.EndVertical();
    }

    [MenuItem("Assets/Create/URP Lit Material from Texture", priority = 0)]
    public static void CreateURPLitMaterial()
    {
        Object[] selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        foreach (Object obj in selectedTextures)
        {
            Texture2D albedoTex = obj as Texture2D;
            if (albedoTex == null) continue;

            // マテリアル作成
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = "mat_"+albedoTex.name.Replace("_d","").Replace("tex_","");

            // Albedo 設定
            material.SetTexture("_BaseMap", albedoTex);
            material.SetColor("_BaseColor", new Color(0.7f, 0.7f, 0.7f, 1f));
            material.SetFloat("_Smoothness", 0);

            // ノーマルマップを探す
            string albedoPath = AssetDatabase.GetAssetPath(albedoTex);
            string folder = Path.GetDirectoryName(albedoPath);
            string baseName = Path.GetFileNameWithoutExtension(albedoPath.Replace("_d",""));

            // 一般的なノーマルマップ名のパターンを試す
            string[] normalSuffixes = new string[] { "_Normal", "_normal", "_n", "Normal" };
            Texture2D normalTex = null;

            foreach (string suffix in normalSuffixes)
            {
                string normalPath = Path.Combine(folder, baseName + suffix + ".png");
                normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (normalTex != null) break;

                normalPath = Path.Combine(folder, baseName + suffix + ".tga");
                normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (normalTex != null) break;

                normalPath = Path.Combine(folder, baseName + suffix + ".jpg");
                normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (normalTex != null) break;
            }

            // ノーマルマップがあれば設定
            if (normalTex != null)
            {
                // ノーマルマップとしてインポート設定されているか確認
                string normalAssetPath = AssetDatabase.GetAssetPath(normalTex);
                TextureImporter importer = AssetImporter.GetAtPath(normalAssetPath) as TextureImporter;

                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }

                material.SetTexture("_BumpMap", normalTex);
            }

            // マテリアルを保存
            string materialPath = Path.Combine(folder, material.name + ".mat");
            AssetDatabase.CreateAsset(material, materialPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
