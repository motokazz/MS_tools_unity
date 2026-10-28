using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MaterialBatchGenerator : EditorWindow
{
    private DefaultAsset inputFolder;
    private DefaultAsset outputFolder;
    private Shader selectedShader;

    private Vector2 scrollPos;

    // マテリアル命名規則
    private string materialPrefix = "M_";
    private string materialSuffix = "";

    // テクスチャ判定用のSuffix
    private string suffixBasemap = "_bc";
    private string suffixNormal = "_n"; // 要望には無かったですが基本使うため追加
    private string suffixORM = "_orm";
    private string suffixMetallic = "_m";
    private string suffixSmoothness = "_s";
    private string suffixRoughness = "_r";
    private string suffixEmissive = "_e";
    private string suffixHeight = "_h";

    // 前回の流れを汲んだオプション
    private bool allowNoSuffixBasemap = true;

    [MenuItem("MS_Tools/Materials/Material Batch Generator")]
    public static void ShowWindow()
    {
        GetWindow<MaterialBatchGenerator>("Material Gen");
    }

    private void OnEnable()
    {
        // デフォルトシェーダーをURP LitかStandardに設定
        selectedShader = Shader.Find("Universal Render Pipeline/Lit");
        if (selectedShader == null) selectedShader = Shader.Find("Standard");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("入出力とシェーダー設定", EditorStyles.boldLabel);
        inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("入力フォルダ (テクスチャ)", inputFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("出力フォルダ (マテリアル)", outputFolder, typeof(DefaultAsset), false);

        GUILayout.Space(5);
        selectedShader = (Shader)EditorGUILayout.ObjectField("適用するシェーダー", selectedShader, typeof(Shader), false);

        GUILayout.Space(15);
        GUILayout.Label("出力マテリアルの命名規則", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        materialPrefix = EditorGUILayout.TextField("Prefix (前)", materialPrefix);
        materialSuffix = EditorGUILayout.TextField("Suffix (後)", materialSuffix);
        GUILayout.EndHorizontal();
        EditorGUILayout.HelpBox($"例: [Prefix] + ベース名 + [Suffix] \n「{materialPrefix}Wall_A{materialSuffix}.mat」のようになります。", MessageType.Info);

        GUILayout.Space(15);
        GUILayout.Label("テクスチャのSuffix設定 (末尾文字)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("空欄にした項目は判定から除外されます。", MessageType.None);

        suffixBasemap = EditorGUILayout.TextField("BaseMap", suffixBasemap);

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        allowNoSuffixBasemap = EditorGUILayout.ToggleLeft("Suffixが無いテクスチャもBaseMapとして扱う", allowNoSuffixBasemap);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        suffixNormal = EditorGUILayout.TextField("Normal", suffixNormal);
        suffixORM = EditorGUILayout.TextField("ORM", suffixORM);
        suffixMetallic = EditorGUILayout.TextField("Metallic", suffixMetallic);
        suffixSmoothness = EditorGUILayout.TextField("Smoothness", suffixSmoothness);
        suffixRoughness = EditorGUILayout.TextField("Roughness", suffixRoughness);
        suffixEmissive = EditorGUILayout.TextField("Emissive", suffixEmissive);
        suffixHeight = EditorGUILayout.TextField("Height", suffixHeight);

        GUILayout.Space(20);

        if (GUILayout.Button("マテリアルを一括生成", GUILayout.Height(40)))
        {
            RunBatchProcess();
        }

        EditorGUILayout.EndScrollView();
    }

    private void RunBatchProcess()
    {
        if (inputFolder == null || outputFolder == null)
        {
            EditorUtility.DisplayDialog("エラー", "入力と出力フォルダを指定してください。", "OK");
            return;
        }
        if (selectedShader == null)
        {
            EditorUtility.DisplayDialog("エラー", "シェーダーを指定してください。", "OK");
            return;
        }

        string inputPath = AssetDatabase.GetAssetPath(inputFolder);
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { inputPath });

        Dictionary<string, TextureGroup> groups = new Dictionary<string, TextureGroup>();

        // テクスチャをBaseName（Suffixを除いた名前）でグループ化
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            if (tex == null) continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string baseName;
            string category = "none";

            // Suffixの判定（長いものから判定すると誤爆を防ぎやすいが、ここでは順番にチェック）
            if (CheckMatch(fileName, suffixBasemap, out baseName)) category = "basemap";
            else if (CheckMatch(fileName, suffixORM, out baseName)) category = "orm";
            else if (CheckMatch(fileName, suffixMetallic, out baseName)) category = "metallic";
            else if (CheckMatch(fileName, suffixSmoothness, out baseName)) category = "smoothness";
            else if (CheckMatch(fileName, suffixRoughness, out baseName)) category = "roughness";
            else if (CheckMatch(fileName, suffixEmissive, out baseName)) category = "emissive";
            else if (CheckMatch(fileName, suffixHeight, out baseName)) category = "height";
            else if (CheckMatch(fileName, suffixNormal, out baseName)) category = "normal";
            else if (allowNoSuffixBasemap)
            {
                baseName = fileName;
                category = "basemap";
            }

            if (category != "none")
            {
                if (!groups.ContainsKey(baseName)) groups[baseName] = new TextureGroup();
                TextureGroup grp = groups[baseName];

                switch (category)
                {
                    case "basemap": grp.basemap = tex; break;
                    case "orm": grp.orm = tex; break;
                    case "metallic": grp.metallic = tex; break;
                    case "smoothness": grp.smoothness = tex; break;
                    case "roughness": grp.roughness = tex; break;
                    case "emissive": grp.emissive = tex; break;
                    case "height": grp.height = tex; break;
                    case "normal": grp.normal = tex; break;
                }
            }
        }

        int successCount = 0;
        int currentIndex = 0;

        try
        {
            foreach (var kvp in groups)
            {
                string baseName = kvp.Key;
                TextureGroup grp = kvp.Value;

                EditorUtility.DisplayProgressBar("マテリアル生成中", $"{baseName} のマテリアルを作成中...", (float)currentIndex / groups.Count);

                if (CreateMaterial(baseName, grp, outputPath))
                {
                    successCount++;
                }

                currentIndex++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完了", $"{groups.Count}個のグループを発見し、\n{successCount}個のマテリアルを生成しました。", "OK");
    }

    private bool CheckMatch(string fileName, string suffix, out string baseName)
    {
        baseName = fileName;
        if (string.IsNullOrEmpty(suffix)) return false;

        if (fileName.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
        {
            baseName = fileName.Substring(0, fileName.Length - suffix.Length);
            return true;
        }
        return false;
    }

    private bool CreateMaterial(string baseName, TextureGroup grp, string outPath)
    {
        string matName = materialPrefix + baseName + materialSuffix + ".mat";
        string matPath = Path.Combine(outPath, matName).Replace("\\", "/");

        Material mat = new Material(selectedShader);

        // --- シェーダーのプロパティにテクスチャを割り当て ---
        // ※ 複数のプロパティ名候補を用意し、URPとBuilt-in Standardの両方に対応しやすくしています。

        // BaseMap
        AssignTexture(mat, grp.basemap, new[] { "_BaseMap", "_MainTex", "_Albedo" });

        // Normal
        AssignTexture(mat, grp.normal, new[] { "_BumpMap", "_NormalMap", "_Normal" });

        // Metallic / ORM
        // URP/StandardはMetallicGlossMap、HDRP等やカスタムシェーダーはMaskMapやORMMapなどを持つ
        if (grp.orm != null)
        {
            AssignTexture(mat, grp.orm, new[] { "_MaskMap", "_ORMMap", "_MetallicGlossMap" });
        }
        else if (grp.metallic != null)
        {
            AssignTexture(mat, grp.metallic, new[] { "_MetallicGlossMap", "_MetallicMap", "_Metallic" });
        }

        // Smoothness / Roughness (カスタムシェーダー用フォールバックとして)
        if (grp.smoothness != null) AssignTexture(mat, grp.smoothness, new[] { "_SmoothnessMap", "_GlossMap", "_Smoothness" });
        if (grp.roughness != null) AssignTexture(mat, grp.roughness, new[] { "_RoughnessMap", "_Roughness" });

        // Emissive
        if (grp.emissive != null)
        {
            bool assigned = AssignTexture(mat, grp.emissive, new[] { "_EmissionMap", "_EmissiveColorMap" });
            if (assigned)
            {
                // Emissiveを光らせるためのキーワード有効化（Standard/URP用）
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.white);
                }
            }
        }

        // Height
        AssignTexture(mat, grp.height, new[] { "_ParallaxMap", "_HeightMap", "_Height" });

        // マテリアルアセットの保存
        AssetDatabase.CreateAsset(mat, matPath);
        return true;
    }

    // マテリアルが指定したプロパティ名を持っているか確認し、持っていればテクスチャをセットする
    private bool AssignTexture(Material mat, Texture tex, string[] propertyNames)
    {
        if (tex == null) return false;

        foreach (string prop in propertyNames)
        {
            if (mat.HasProperty(prop))
            {
                mat.SetTexture(prop, tex);
                return true;
            }
        }
        return false;
    }

    private class TextureGroup
    {
        public Texture basemap, normal, orm, metallic, smoothness, roughness, emissive, height;
    }
}