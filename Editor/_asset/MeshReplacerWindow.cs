using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class MeshReplacerWindow : EditorWindow
{
    // メッシュ用設定
    private DefaultAsset targetModelFolder;

    // マテリアル用設定
    private DefaultAsset targetMaterialFolder;
    private bool usePartialMatchForMaterial = false;
    private string ignoreMaterialString = "(Instance)"; // よくある (Instance) をデフォルトに

    [MenuItem("MS_Tools/Models/Mesh & Material Replacer")]
    public static void ShowWindow()
    {
        GetWindow<MeshReplacerWindow>("Mesh & Material Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("1. 対象フォルダをセット（サブフォルダも自動検索されます）\n2. Projectビューでプレファブを選択\n3. 実行ボタンを押す", EditorStyles.helpBox);

        EditorGUILayout.Space();

        // --- メッシュ設定 ---
        GUILayout.Label("◆ メッシュの差し替え", EditorStyles.boldLabel);
        targetModelFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Model Folder", targetModelFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space();

        // --- マテリアル設定 ---
        GUILayout.Label("◆ マテリアルの差し替え", EditorStyles.boldLabel);
        targetMaterialFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Material Folder", targetMaterialFolder, typeof(DefaultAsset), false);

        EditorGUI.indentLevel++;
        usePartialMatchForMaterial = EditorGUILayout.Toggle("名前の部分一致を許可", usePartialMatchForMaterial);
        ignoreMaterialString = EditorGUILayout.TextField("比較時に無視する文字 (カンマ区切)", ignoreMaterialString);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        bool noFolderSelected = targetModelFolder == null && targetMaterialFolder == null;
        bool noPrefabSelected = Selection.GetFiltered<GameObject>(SelectionMode.Assets).Length == 0;

        EditorGUI.BeginDisabledGroup(noFolderSelected || noPrefabSelected);
        if (GUILayout.Button("Replace in Selected Prefabs", GUILayout.Height(30)))
        {
            ReplaceMeshesAndMaterials();
        }
        EditorGUI.EndDisabledGroup();

        if (noFolderSelected)
        {
            EditorGUILayout.HelpBox("メッシュ用かマテリアル用の少なくとも1つのフォルダを指定してください。", MessageType.Info);
        }
    }

    private void ReplaceMeshesAndMaterials()
    {
        // === 1. 置換用データの収集 ===
        Dictionary<string, Mesh> newMeshes = new Dictionary<string, Mesh>();
        List<Material> newMaterials = new List<Material>();

        if (targetModelFolder != null)
        {
            string modelFolderPath = AssetDatabase.GetAssetPath(targetModelFolder);
            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { modelFolderPath });
            foreach (string guid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object asset in assets)
                {
                    if (asset is Mesh mesh && !newMeshes.ContainsKey(mesh.name))
                    {
                        newMeshes.Add(mesh.name, mesh);
                    }
                }
            }
        }

        if (targetMaterialFolder != null)
        {
            string matFolderPath = AssetDatabase.GetAssetPath(targetMaterialFolder);
            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { matFolderPath });
            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object asset in assets)
                {
                    if (asset is Material mat && !newMaterials.Contains(mat))
                    {
                        newMaterials.Add(mat);
                    }
                }
            }
        }

        // 無視する文字列のリスト化（空白削除して空文字を除外）
        List<string> ignoreKeywords = ignoreMaterialString.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        // === 2. プレファブの処理 ===
        GameObject[] selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        int replacedPrefabCount = 0;
        int totalReplacedMeshes = 0;
        int totalReplacedMaterials = 0;

        foreach (GameObject prefab in selectedPrefabs)
        {
            if (!PrefabUtility.IsPartOfPrefabAsset(prefab)) continue;

            string prefabPath = AssetDatabase.GetAssetPath(prefab);

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = scope.prefabContentsRoot;
                bool modified = false;

                // --- メッシュの差し替え ---
                if (newMeshes.Count > 0)
                {
                    foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (mf.sharedMesh != null && newMeshes.TryGetValue(mf.sharedMesh.name, out Mesh replacement))
                        {
                            if (mf.sharedMesh != replacement)
                            {
                                mf.sharedMesh = replacement;
                                modified = true;
                                totalReplacedMeshes++;
                            }
                        }
                    }

                    foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (smr.sharedMesh != null && newMeshes.TryGetValue(smr.sharedMesh.name, out Mesh replacement))
                        {
                            if (smr.sharedMesh != replacement)
                            {
                                smr.sharedMesh = replacement;
                                modified = true;
                                totalReplacedMeshes++;
                            }
                        }
                    }
                }

                // --- マテリアルの差し替え ---
                if (newMaterials.Count > 0)
                {
                    Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer renderer in renderers)
                    {
                        Material[] sharedMats = renderer.sharedMaterials;
                        bool matsModified = false;

                        for (int i = 0; i < sharedMats.Length; i++)
                        {
                            Material currentMat = sharedMats[i];
                            if (currentMat == null) continue;

                            // 比較用文字列を元にマッチするマテリアルを検索
                            Material replacementMat = FindMatchingMaterial(currentMat.name, newMaterials, usePartialMatchForMaterial, ignoreKeywords);

                            // 差し替え
                            if (replacementMat != null && currentMat != replacementMat)
                            {
                                sharedMats[i] = replacementMat;
                                matsModified = true;
                                totalReplacedMaterials++;
                            }
                        }

                        if (matsModified)
                        {
                            renderer.sharedMaterials = sharedMats;
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    replacedPrefabCount++;
                }
            }
        }

        Debug.Log($"完了: {replacedPrefabCount}個のプレファブ内で、メッシュ{totalReplacedMeshes}個、マテリアル{totalReplacedMaterials}箇所を差し替えました。");
    }

    // 指定されたキーワード群を文字列から取り除いた（無視した）クリーンな名前を返す
    private string CleanName(string originalName, List<string> ignoreKeywords)
    {
        string cleaned = originalName;
        foreach (string keyword in ignoreKeywords)
        {
            // 大文字小文字を無視してキーワードを空文字に置換（取り除く）
            cleaned = Regex.Replace(cleaned, Regex.Escape(keyword), "", RegexOptions.IgnoreCase);
        }
        return cleaned.Trim(); // 先頭・末尾の余計な空白も消す
    }

    // マッチするマテリアルを探すメソッド
    private Material FindMatchingMaterial(string targetName, List<Material> sourceMaterials, bool partialMatch, List<string> ignoreKeywords)
    {
        // プレファブ側のマテリアル名から無視文字を取り除く
        string cleanedTarget = CleanName(targetName, ignoreKeywords);

        // 取り除いた結果、名前が空になってしまった場合は誤判定を防ぐためスキップ
        if (string.IsNullOrEmpty(cleanedTarget)) return null;

        foreach (Material mat in sourceMaterials)
        {
            // 対象フォルダ側のマテリアル名からも無視文字を取り除く
            string cleanedSource = CleanName(mat.name, ignoreKeywords);

            if (string.IsNullOrEmpty(cleanedSource)) continue;

            if (partialMatch)
            {
                // 部分一致判定
                if (cleanedSource.IndexOf(cleanedTarget, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    cleanedTarget.IndexOf(cleanedSource, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mat;
                }
            }
            else
            {
                // 完全一致（大文字小文字無視）判定
                if (string.Equals(cleanedSource, cleanedTarget, System.StringComparison.OrdinalIgnoreCase))
                {
                    return mat;
                }
            }
        }
        return null;
    }
}