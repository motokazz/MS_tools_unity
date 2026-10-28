using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public enum TextureChannel
{
    RGB, R, G, B, A, RGBA
}

public class TextureRenameCopier : EditorWindow
{
    private DefaultAsset inputFolder;
    private DefaultAsset outputFolder;
    private string commonPrefix = "";
    private Vector2 scrollPos;

    // --- 単体コピー・リネーム用のルール ---
    private string keyBasemap = "basemap";
    private bool allowNoSuffixBasemap = true; // 追加：サフィックス無しを許容するか
    private string keyORM = "orm";
    private string keyNormal = "normal";
    private string keyEmissive = "emissive";
    private string keyHeight = "height";

    private string suffixBasemap = "_bc";
    private string suffixORM = "_orm";
    private string suffixNormal = "_n";
    private string suffixEmissive = "_e";
    private string suffixHeight = "_h";

    // --- ORM合成用のルール ---
    private string keyOcclusion = "ao";
    private string keyRoughness = "smoothness";
    private string keyMetallic = "metallic";

    private TextureChannel channelOcclusion = TextureChannel.R;
    private TextureChannel channelRoughness = TextureChannel.A;
    private TextureChannel channelMetallic = TextureChannel.R;

    private bool invertRoughness = true;

    [MenuItem("MS_Tools/Textures/Texture Rename & ORM Copier")]
    public static void ShowWindow()
    {
        GetWindow<TextureRenameCopier>("Tex Copier");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("フォルダ設定", EditorStyles.boldLabel);
        inputFolder = (DefaultAsset)EditorGUILayout.ObjectField("入力フォルダ", inputFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("出力フォルダ (必須)", outputFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("リネーム設定", EditorStyles.boldLabel);
        commonPrefix = EditorGUILayout.TextField("共通Prefix (任意)", commonPrefix);

        GUILayout.Space(15);
        GUILayout.Label("① 単体テクスチャのコピー・リネーム設定", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("判定元 (_以降)", GUILayout.Width(130));
        GUILayout.Label("⇒");
        GUILayout.Label("複製後の末尾 (Suffix)", GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        DrawRuleField(ref keyBasemap, ref suffixBasemap);

        // BaseMap用の特別オプション
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        allowNoSuffixBasemap = EditorGUILayout.ToggleLeft("「_〇〇」等のサフィックスが無いファイル名も BaseMap として扱う", allowNoSuffixBasemap);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        DrawRuleField(ref keyORM, ref suffixORM);
        DrawRuleField(ref keyNormal, ref suffixNormal);
        DrawRuleField(ref keyEmissive, ref suffixEmissive);
        DrawRuleField(ref keyHeight, ref suffixHeight);

        GUILayout.Space(15);
        GUILayout.Label("② ORMマップの合成設定", EditorStyles.boldLabel);
        DrawSynthesisField("Occlusion (Rへ)", ref keyOcclusion, ref channelOcclusion);
        GUILayout.Space(5);
        DrawSynthesisField("Rough/Smooth (Gへ)", ref keyRoughness, ref channelRoughness);
        invertRoughness = EditorGUILayout.ToggleLeft("数値を反転する (Smoothness → Roughness変換用)", invertRoughness);
        GUILayout.Space(5);
        DrawSynthesisField("Metallic (Bへ)", ref keyMetallic, ref channelMetallic);

        GUILayout.Space(20);

        if (GUILayout.Button("条件に合うテクスチャを処理", GUILayout.Height(40)))
        {
            RunBatchProcess();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRuleField(ref string key, ref string suffix)
    {
        GUILayout.BeginHorizontal();
        key = EditorGUILayout.TextField(key, GUILayout.Width(130));
        GUILayout.Label("⇒", GUILayout.Width(20));
        suffix = EditorGUILayout.TextField(suffix);
        GUILayout.EndHorizontal();
    }

    private void DrawSynthesisField(string label, ref string key, ref TextureChannel channel)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(130));
        key = EditorGUILayout.TextField(key, GUILayout.Width(90));
        GUILayout.Label("ﾁｬﾝﾈﾙ:", GUILayout.Width(45));
        channel = (TextureChannel)EditorGUILayout.EnumPopup(channel);
        GUILayout.EndHorizontal();
    }

    private void RunBatchProcess()
    {
        if (inputFolder == null || outputFolder == null)
        {
            EditorUtility.DisplayDialog("エラー", "入力と出力フォルダを指定してください。", "OK");
            return;
        }

        string inputPath = AssetDatabase.GetAssetPath(inputFolder);
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { inputPath });

        Dictionary<string, TextureGroup> groups = new Dictionary<string, TextureGroup>();

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex == null) continue;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string baseName;
            string category = "none";

            // 末尾が一致するか判定していく
            if (CheckMatch(fileName, keyBasemap, out baseName)) category = "basemap";
            else if (CheckMatch(fileName, keyORM, out baseName)) category = "orm";
            else if (CheckMatch(fileName, keyNormal, out baseName)) category = "normal";
            else if (CheckMatch(fileName, keyEmissive, out baseName)) category = "emissive";
            else if (CheckMatch(fileName, keyHeight, out baseName)) category = "height";
            else if (CheckMatch(fileName, keyOcclusion, out baseName)) category = "occlusion";
            else if (CheckMatch(fileName, keyRoughness, out baseName)) category = "roughness";
            else if (CheckMatch(fileName, keyMetallic, out baseName)) category = "metallic";
            else if (allowNoSuffixBasemap)
            {
                // どのルールにも合致せず、サフィックス無し許可設定がONなら、ファイル名自体をベース名としてBaseMapに
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
                    case "normal": grp.normal = tex; break;
                    case "emissive": grp.emissive = tex; break;
                    case "height": grp.height = tex; break;
                    case "occlusion": grp.occlusion = tex; break;
                    case "roughness": grp.roughness = tex; break;
                    case "metallic": grp.metallic = tex; break;
                }
            }
        }

        int copiedCount = 0;
        int synthesizedCount = 0;
        int currentIndex = 0;

        try
        {
            foreach (var kvp in groups)
            {
                string baseName = kvp.Key;
                TextureGroup grp = kvp.Value;

                EditorUtility.DisplayProgressBar("一括処理中", $"{baseName} を処理しています...", (float)currentIndex / groups.Count);

                if (grp.basemap != null) copiedCount += CopyTex(grp.basemap, baseName, suffixBasemap, outputPath);
                if (grp.normal != null) copiedCount += CopyTex(grp.normal, baseName, suffixNormal, outputPath);
                if (grp.emissive != null) copiedCount += CopyTex(grp.emissive, baseName, suffixEmissive, outputPath);
                if (grp.height != null) copiedCount += CopyTex(grp.height, baseName, suffixHeight, outputPath);

                bool doSynthesis = (grp.occlusion != null || grp.roughness != null || grp.metallic != null);
                if (doSynthesis)
                {
                    if (GenerateORMMap(baseName, grp, outputPath)) synthesizedCount++;
                }
                else if (grp.orm != null)
                {
                    copiedCount += CopyTex(grp.orm, baseName, suffixORM, outputPath);
                }

                currentIndex++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完了", $"処理が完了しました。\n\nコピー・リネーム数: {copiedCount}件\n合成したORMマップ数: {synthesizedCount}件", "OK");
    }

    // ファイル名が「_」+「指定キー」で終わっているかチェックし、終わっていればベース名を取り出す
    private bool CheckMatch(string fileName, string key, out string baseName)
    {
        baseName = fileName;
        if (string.IsNullOrWhiteSpace(key)) return false;

        string suffix = "_" + key;
        if (fileName.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
        {
            baseName = fileName.Substring(0, fileName.Length - suffix.Length);
            return true;
        }
        return false;
    }

    private int CopyTex(Texture2D tex, string baseName, string targetSuffix, string outPath)
    {
        string assetPath = AssetDatabase.GetAssetPath(tex);
        string extension = Path.GetExtension(assetPath);
        string newName = commonPrefix + baseName + targetSuffix + extension;
        string finalPath = Path.Combine(outPath, newName).Replace("\\", "/");

        return AssetDatabase.CopyAsset(assetPath, finalPath) ? 1 : 0;
    }

    private bool GenerateORMMap(string baseName, TextureGroup group, string outputFolderPath)
    {
        Texture2D refTex = group.occlusion != null ? group.occlusion :
                           (group.roughness != null ? group.roughness : group.metallic);

        int width = refTex.width;
        int height = refTex.height;

        MakeTextureReadable(group.occlusion);
        MakeTextureReadable(group.roughness);
        MakeTextureReadable(group.metallic);

        Color[] occPixels = group.occlusion ? group.occlusion.GetPixels() : null;
        Color[] rouPixels = group.roughness ? group.roughness.GetPixels() : null;
        Color[] metPixels = group.metallic ? group.metallic.GetPixels() : null;

        Texture2D ormTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] ormPixels = new Color[width * height];

        for (int i = 0; i < ormPixels.Length; i++)
        {
            float o = 1.0f;
            float r = 0.5f;
            float m = 0.0f;

            if (occPixels != null && i < occPixels.Length) o = GetChannelValue(occPixels[i], channelOcclusion);
            if (metPixels != null && i < metPixels.Length) m = GetChannelValue(metPixels[i], channelMetallic);

            if (rouPixels != null && i < rouPixels.Length)
            {
                r = GetChannelValue(rouPixels[i], channelRoughness);
                if (invertRoughness) r = 1.0f - r;
            }

            ormPixels[i] = new Color(o, r, m, 1.0f);
        }

        ormTex.SetPixels(ormPixels);
        ormTex.Apply();

        string finalPath = Path.Combine(outputFolderPath, commonPrefix + baseName + suffixORM + ".png").Replace("\\", "/");
        File.WriteAllBytes(finalPath, ormTex.EncodeToPNG());

        AssetDatabase.ImportAsset(finalPath);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(finalPath);
        if (importer != null)
        {
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }

        return true;
    }

    private float GetChannelValue(Color c, TextureChannel channel)
    {
        switch (channel)
        {
            case TextureChannel.R: return c.r;
            case TextureChannel.G: return c.g;
            case TextureChannel.B: return c.b;
            case TextureChannel.A: return c.a;
            case TextureChannel.RGB: return (c.r + c.g + c.b) / 3f;
            case TextureChannel.RGBA: return (c.r + c.g + c.b + c.a) / 4f;
            default: return c.r;
        }
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
        public Texture2D basemap, orm, normal, emissive, height;
        public Texture2D occlusion, roughness, metallic;
    }
}