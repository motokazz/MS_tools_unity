using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SmartShaderReplacer : EditorWindow
{
    private Shader targetShader;
    private Vector2 scrollPos;
    private bool showHelp = false;

    // ==========================================
    // 1. テクスチャ推測ルールの定義
    // ==========================================
    private enum TexCategory
    {
        Base,
        Normal,
        Metallic,
        Emission,
        Occlusion,
        Detail
    }

    private class TexCategoryRule
    {
        public TexCategory category;
        public string[] exactMatches;   // 完全一致（優先度：高）
        public string[] partialMatches; // 部分一致（優先度：低）
    }

    private readonly List<TexCategoryRule> texCategoryRules = new List<TexCategoryRule>()
    {
        new TexCategoryRule
        {
            category = TexCategory.Base,
            exactMatches = new[] { "_maintex", "_basemap", "_basecolormap", "_albedomap" , "_d" ,"_b"},
            partialMatches = new[] { "base", "albedo", "main" ,"diffuse"}
        },
        new TexCategoryRule
        {
            category = TexCategory.Normal,
            exactMatches = new[] { "_bumpmap", "_normalmap" },
            partialMatches = new[] { "normal", "bump" }
        },
        new TexCategoryRule
        {
            category = TexCategory.Metallic,
            exactMatches = new[] { "_metallicglossmap", "_metallicmap", "_maskmap" ,"_orm" ,"_mro"},
            partialMatches = new[] { "metallic", "mask", "spec" ,"orm" , "mro"}
        },
        new TexCategoryRule
        {
            category = TexCategory.Emission,
            exactMatches = new[] { "_emissionmap" },
            partialMatches = new[] { "emission", "emissive" }
        },
        new TexCategoryRule
        {
            category = TexCategory.Occlusion,
            exactMatches = new[] { "_occlusionmap" },
            partialMatches = new[] { "occlusion", "ao" }
        },
        new TexCategoryRule
        {
            category = TexCategory.Detail,
            exactMatches = new string[] { },
            partialMatches = new[] { "detail" }
        }
    };

    // ==========================================
    // 2. カラープロパティ推測ルールの定義
    // ==========================================
    private enum ColorCategory
    {
        BaseColor,
        EmissionColor
    }

    private class ColorRule
    {
        public ColorCategory category;
        public string[] exactMatches;   // 完全一致（優先度：高）
        public string[] partialMatches; // 部分一致（優先度：低）
    }

    private readonly List<ColorRule> colorRules = new List<ColorRule>()
    {
        new ColorRule
        {
            category = ColorCategory.BaseColor,
            exactMatches = new[] { "_basecolor", "_color", "_maincolor", "_tint" },
            // ※注意: "color" を部分一致に入れると _EmissionColor なども誤爆するため、
            // base, main, albedo などのキーワードで推測させます。
            partialMatches = new[] { "base", "main", "albedo", "tint" }
        },
        new ColorRule
        {
            category = ColorCategory.EmissionColor,
            exactMatches = new[] { "_emissioncolor", "_emissivecolor", "_emission" },
            partialMatches = new[] { "emission", "emissive" }
        }
    };
    // ==========================================


    [MenuItem("MS_Tools/Materials/Smart Shader Replacer")]
    public static void ShowWindow()
    {
        GetWindow<SmartShaderReplacer>("Shader Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Smart Shader Replacer", EditorStyles.boldLabel);

        showHelp = EditorGUILayout.Foldout(showHelp, "使い方 (How to use)", true);
        if (showHelp)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "1. Project/Hierarchyビューで、対象のマテリアルを選択します（複数可）。\n" +
                "2. 下の「Target Shader」に変更先のシェーダーを設定します。\n" +
                "3. 「Replace Shader...」ボタンを押すと一括で変換されます。\n\n" +
                "【特徴】プロパティ名からBase, Normal, Metallicなどを自動推測し、テクスチャや色を維持します。\n" +
                "【取り消し】実行後は Ctrl+Z (MacはCmd+Z) で元に戻せます。",
                MessageType.Info);
            EditorGUI.indentLevel--;
            GUILayout.Space(5);
        }

        GUILayout.Space(5);
        targetShader = (Shader)EditorGUILayout.ObjectField("Target Shader", targetShader, typeof(Shader), false);

        GUILayout.Space(10);
        GUILayout.Label("Selected Materials:", EditorStyles.boldLabel);

        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.Height(150));
        if (selectedObjects.Length == 0)
        {
            EditorGUILayout.LabelField("マテリアルが選択されていません", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            foreach (var obj in selectedObjects)
            {
                EditorGUILayout.LabelField($"・ {obj.name}");
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        GUI.enabled = targetShader != null && selectedObjects.Length > 0;
        if (GUILayout.Button("Replace Shader & Restore Properties", GUILayout.Height(40)))
        {
            ReplaceShaders(selectedObjects);
        }
        GUI.enabled = true;
    }

    private void ReplaceShaders(Object[] materials)
    {
        Undo.RecordObjects(materials, "Replace Shaders");
        int replacedCount = 0;

        foreach (Material mat in materials)
        {
            if (mat == null) continue;

            Dictionary<TexCategory, Texture> backupTextures = new Dictionary<TexCategory, Texture>();
            Dictionary<ColorCategory, Color> backupColors = new Dictionary<ColorCategory, Color>();

            // --- 1. カラープロパティのバックアップ ---
            int propCount = mat.shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                // 色に関するプロパティだけを抽出
                if (mat.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    string propName = mat.shader.GetPropertyName(i);
                    ColorCategory? category = GuessColorCategory(propName);

                    if (category.HasValue && !backupColors.ContainsKey(category.Value))
                    {
                        backupColors[category.Value] = mat.GetColor(propName);
                    }
                }
            }

            // --- 2. テクスチャのバックアップ ---
            string[] oldProps = mat.GetTexturePropertyNames();
            foreach (string prop in oldProps)
            {
                Texture tex = mat.GetTexture(prop);
                if (tex == null) continue;

                TexCategory? category = GuessTexCategory(prop);
                if (category.HasValue && !backupTextures.ContainsKey(category.Value))
                {
                    backupTextures[category.Value] = tex;
                }
            }

            // --- 3. シェーダーの変更 ---
            mat.shader = targetShader;

            // --- 4. カラープロパティの復元 ---
            int newPropCount = mat.shader.GetPropertyCount();
            for (int i = 0; i < newPropCount; i++)
            {
                if (mat.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    string propName = mat.shader.GetPropertyName(i);
                    ColorCategory? category = GuessColorCategory(propName);

                    if (category.HasValue && backupColors.ContainsKey(category.Value))
                    {
                        mat.SetColor(propName, backupColors[category.Value]);
                        backupColors.Remove(category.Value);
                    }
                }
            }

            // --- 5. テクスチャの復元 ---
            string[] newProps = mat.GetTexturePropertyNames();
            foreach (string prop in newProps)
            {
                TexCategory? category = GuessTexCategory(prop);
                if (category.HasValue && backupTextures.ContainsKey(category.Value))
                {
                    mat.SetTexture(prop, backupTextures[category.Value]);
                    backupTextures.Remove(category.Value);
                }
            }
            replacedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Smart Shader Replacer] {replacedCount}個のマテリアルを変換しました。");
    }

    // --- テクスチャの推測ロジック ---
    private TexCategory? GuessTexCategory(string propName)
    {
        string lower = propName.ToLower();

        foreach (var rule in texCategoryRules)
        {
            foreach (var match in rule.exactMatches)
            {
                if (lower == match) return rule.category;
            }
        }

        foreach (var rule in texCategoryRules)
        {
            foreach (var match in rule.partialMatches)
            {
                if (lower.Contains(match)) return rule.category;
            }
        }

        return null;
    }

    // --- カラーの推測ロジック ---
    private ColorCategory? GuessColorCategory(string propName)
    {
        string lower = propName.ToLower();

        // 1. 完全一致
        foreach (var rule in colorRules)
        {
            foreach (var match in rule.exactMatches)
            {
                if (lower == match) return rule.category;
            }
        }

        // 2. 部分一致
        foreach (var rule in colorRules)
        {
            foreach (var match in rule.partialMatches)
            {
                if (lower.Contains(match)) return rule.category;
            }
        }

        return null;
    }
}