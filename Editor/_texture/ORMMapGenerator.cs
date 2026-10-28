using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ORMMapBatchGenerator : EditorWindow
{
    private DefaultAsset inputFolder;
    private DefaultAsset outputFolder;

    // ファイル名末尾のルール設定
    private string occlusionSuffix = "_ao";
    private string roughnessSuffix = "_r";
    private string metallicSuffix = "_m";
    private string outputSuffix = "_orm";

    [MenuItem("Tools/ORM Map Batch Generator")]
    public static void ShowWindow()
    {
        GetWindow<ORMMapBatchGenerator>("ORM Batch Gen");
    }

    void OnGUI()
    {
        GUILayout.Label("フォルダ設定", EditorStyles.boldLabel);
        inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("入力フォルダ", inputFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("出力フォルダ (空なら入力と同じ)", outputFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("ファイル名ルールの設定 (末尾の文字列)", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Occlusion (R) に使う末尾", GUILayout.Width(180));
        occlusionSuffix = EditorGUILayout.TextField(occlusionSuffix);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Roughness (G) に使う末尾", GUILayout.Width(180));
        roughnessSuffix = EditorGUILayout.TextField(roughnessSuffix);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Metallic (B) に使う末尾", GUILayout.Width(180));
        metallicSuffix = EditorGUILayout.TextField(metallicSuffix);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("出力ファイルの末尾", GUILayout.Width(180));
        outputSuffix = EditorGUILayout.TextField(outputSuffix);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("ORMマップを一括生成", GUILayout.Height(40)))
        {
            RunBatchProcess();
        }
    }

    private void RunBatchProcess()
    {
        if (inputFolder == null)
        {
            EditorUtility.DisplayDialog("エラー", "入力フォルダを指定してください。", "OK");
            return;
        }

        string inputPath = AssetDatabase.GetAssetPath(inputFolder);
        string outputPath = outputFolder != null ? AssetDatabase.GetAssetPath(outputFolder) : inputPath;

        // 指定フォルダ内のすべてのテクスチャを検索
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { inputPath });

        // ベースファイル名をキーにしてペアを管理
        Dictionary<string, TextureGroup> textureGroups = new Dictionary<string, TextureGroup>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null) continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (fileName.EndsWith(occlusionSuffix, System.StringComparison.OrdinalIgnoreCase))
            {
                string baseName = fileName.Substring(0, fileName.Length - occlusionSuffix.Length);
                GetOrCreateGroup(textureGroups, baseName).occlusionTex = tex;
            }
            else if (fileName.EndsWith(roughnessSuffix, System.StringComparison.OrdinalIgnoreCase))
            {
                string baseName = fileName.Substring(0, fileName.Length - roughnessSuffix.Length);
                GetOrCreateGroup(textureGroups, baseName).roughnessTex = tex;
            }
            else if (fileName.EndsWith(metallicSuffix, System.StringComparison.OrdinalIgnoreCase))
            {
                string baseName = fileName.Substring(0, fileName.Length - metallicSuffix.Length);
                GetOrCreateGroup(textureGroups, baseName).metallicTex = tex;
            }
        }

        int successCount = 0;
        int totalCount = textureGroups.Count;

        if (totalCount == 0)
        {
            EditorUtility.DisplayDialog("結果", "指定されたルールに合致するテクスチャが見つかりませんでした。", "OK");
            return;
        }

        try
        {
            int currentIndex = 0;
            foreach (var kvp in textureGroups)
            {
                string baseName = kvp.Key;
                TextureGroup group = kvp.Value;

                EditorUtility.DisplayProgressBar("ORMマップ生成中", $"{baseName} を処理中...", (float)currentIndex / totalCount);

                if (GenerateORMMap(baseName, group, outputPath))
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
        EditorUtility.DisplayDialog("完了", $"{totalCount}グループ中、{successCount}件のORMマップを生成しました。", "OK");
    }

    private TextureGroup GetOrCreateGroup(Dictionary<string, TextureGroup> dict, string baseName)
    {
        if (!dict.ContainsKey(baseName))
        {
            dict[baseName] = new TextureGroup();
        }
        return dict[baseName];
    }

    private bool GenerateORMMap(string baseName, TextureGroup group, string outputFolderPath)
    {
        // 3つのうちどれか1つでもあれば生成を実行する
        if (group.occlusionTex == null && group.roughnessTex == null && group.metallicTex == null) return false;

        // 解像度の基準を決める（存在するテクスチャのうち最初のもの）
        Texture2D refTex = group.occlusionTex != null ? group.occlusionTex :
                           (group.roughnessTex != null ? group.roughnessTex : group.metallicTex);

        int width = refTex.width;
        int height = refTex.height;

        MakeTextureReadable(group.occlusionTex);
        MakeTextureReadable(group.roughnessTex);
        MakeTextureReadable(group.metallicTex);

        Color[] occPixels = group.occlusionTex ? group.occlusionTex.GetPixels() : null;
        Color[] rouPixels = group.roughnessTex ? group.roughnessTex.GetPixels() : null;
        Color[] metPixels = group.metallicTex ? group.metallicTex.GetPixels() : null;

        Texture2D ormTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] ormPixels = new Color[width * height];

        for (int i = 0; i < ormPixels.Length; i++)
        {
            // O: Occlusion (テクスチャがない場合は 1.0 = 影なし)
            float o = (occPixels != null && i < occPixels.Length) ? occPixels[i].r : 1.0f;

            // R: Roughness (テクスチャがない場合は 0.5 = 中間の粗さ)
            float r = (rouPixels != null && i < rouPixels.Length) ? rouPixels[i].r : 0.5f;

            // M: Metallic (テクスチャがない場合は 0.0 = 非金属)
            float m = (metPixels != null && i < metPixels.Length) ? metPixels[i].r : 0.0f;

            ormPixels[i] = new Color(o, r, m, 1.0f);
        }

        ormTex.SetPixels(ormPixels);
        ormTex.Apply();

        string finalPath = Path.Combine(outputFolderPath, baseName + outputSuffix + ".png");

        byte[] pngData = ormTex.EncodeToPNG();
        File.WriteAllBytes(finalPath, pngData);

        AssetDatabase.ImportAsset(finalPath);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(finalPath);
        if (importer != null)
        {
            importer.sRGBTexture = false; // マスクマップなのでLinear設定
            importer.SaveAndReimport();
        }

        return true;
    }

    private void MakeTextureReadable(Texture2D tex)
    {
        if (tex == null) return;

        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }

    private class TextureGroup
    {
        public Texture2D occlusionTex;
        public Texture2D roughnessTex;
        public Texture2D metallicTex;
    }
}