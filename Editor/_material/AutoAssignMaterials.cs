using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoAssignMaterials : EditorWindow
{
    private DefaultAsset modelFolder;
    private DefaultAsset materialFolder;
    private bool showDebugLog = true; // ログ出力設定

    // 除外したい文字列（カンマ区切りで入力）
    private string ignoreStringsInput = "_Mat, _Material, (Clone)";

    [MenuItem("MS_Tools/Materials/Auto Assign Materials")]
    public static void ShowWindow()
    {
        GetWindow<AutoAssignMaterials>("Auto Assign Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("フォルダ設定", EditorStyles.boldLabel);
        modelFolder = (DefaultAsset)EditorGUILayout.ObjectField("モデルフォルダ (FBX / Prefab)", modelFolder, typeof(DefaultAsset), false);
        materialFolder = (DefaultAsset)EditorGUILayout.ObjectField("マテリアルフォルダ", materialFolder, typeof(DefaultAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("検索オプション", EditorStyles.boldLabel);
        ignoreStringsInput = EditorGUILayout.TextField("除外する文字列 (カンマ区切り)", ignoreStringsInput);

        GUILayout.Space(15);
        EditorGUILayout.HelpBox(
            "【検索ルール（優先順位）】\n" +
            "1. 元モデルが持っているマテリアル名（指定した除外文字列をカット）\n" +
            "2. モデルの名前 (オブジェクト名)\n" +
            "3. 無ければ変更しない\n\n" +
            "※双方向の部分一致で判定します。\n" +
            "※FBXの場合は自動で「Use Embedded Materials」に変更し、元のマテリアル名をキーにしてリマップします。", MessageType.Info);

        GUILayout.Space(10);
        showDebugLog = EditorGUILayout.ToggleLeft("処理結果をコンソール(Console)にログ出力する", showDebugLog);

        GUILayout.Space(20);

        if (GUILayout.Button("マテリアルを一括設定", GUILayout.Height(40)))
        {
            RunAutoAssign();
        }
    }

    private void RunAutoAssign()
    {
        if (modelFolder == null || materialFolder == null)
        {
            EditorUtility.DisplayDialog("エラー", "モデルフォルダとマテリアルフォルダの両方を指定してください。", "OK");
            return;
        }

        string modelPath = AssetDatabase.GetAssetPath(modelFolder);
        string materialPath = AssetDatabase.GetAssetPath(materialFolder);

        // 指定フォルダ内の全マテリアルを取得
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { materialPath });
        List<Material> availableMaterials = new List<Material>();
        foreach (string guid in matGuids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (mat != null) availableMaterials.Add(mat);
        }

        if (availableMaterials.Count == 0)
        {
            EditorUtility.DisplayDialog("エラー", "指定されたマテリアルフォルダ内にマテリアルが見つかりません。", "OK");
            return;
        }

        // 指定フォルダ内の全モデルを取得
        string[] modelGuids = AssetDatabase.FindAssets("t:GameObject", new[] { modelPath });

        int successCount = 0;

        try
        {
            for (int i = 0; i < modelGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                GameObject assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (assetRoot == null) continue;

                EditorUtility.DisplayProgressBar("マテリアル割り当て中", $"{assetRoot.name} を処理しています...", (float)i / modelGuids.Length);

                bool isModelImporter = AssetImporter.GetAtPath(assetPath) is ModelImporter;
                bool changed = false;

                if (isModelImporter)
                {
                    // ===== FBX等のインポートモデルの場合 =====
                    ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

                    if (importer.materialLocation != ModelImporterMaterialLocation.InPrefab)
                    {
                        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                        importer.SaveAndReimport(); // 即座に反映して内部マテリアルを生成させる
                        changed = true;

                        if (showDebugLog)
                            Debug.Log($"[設定変更] {assetRoot.name} のMaterial Locationを 'Use Embedded Materials' に変更しました。");
                    }

                    // 画像左側の「元モデルが持っているマテリアル名」をすべて収集する
                    HashSet<string> originalMatNames = new HashSet<string>();

                    // 1. 未割り当て（Missing等）の元マテリアル名をサブアセットから取得
                    Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (Object obj in subAssets)
                    {
                        if (obj is Material mat)
                        {
                            originalMatNames.Add(mat.name);
                        }
                    }

                    // 2. 既に割り当て（リマップ）済みの元マテリアル名を取得
                    var externalMap = importer.GetExternalObjectMap();
                    foreach (var kvp in externalMap)
                    {
                        if (kvp.Key.type == typeof(Material))
                        {
                            originalMatNames.Add(kvp.Key.name);
                        }
                    }

                    // 収集した元のマテリアル名を使って最適なマテリアルを探し、リマップする
                    foreach (string origMatName in originalMatNames)
                    {
                        Material foundMat = FindMaterial(origMatName, assetRoot.name, availableMaterials, "");

                        if (foundMat != null)
                        {
                            // 既に同じマテリアルが割り当てられているかチェック
                            bool alreadyMapped = false;
                            var identifier = new AssetImporter.SourceAssetIdentifier(typeof(Material), origMatName);
                            if (externalMap.TryGetValue(identifier, out UnityEngine.Object existingMat))
                            {
                                if (existingMat == foundMat) alreadyMapped = true;
                            }

                            if (!alreadyMapped)
                            {
                                importer.AddRemap(identifier, foundMat);
                                changed = true;

                                if (showDebugLog)
                                    Debug.Log($"[FBX更新] {assetRoot.name} の元のマテリアル '{origMatName}' を '{foundMat.name}' に割り当てました。");
                            }
                        }
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        successCount++;
                    }
                    else if (showDebugLog)
                    {
                        Debug.LogWarning($"[スキップ] FBXモデル '{assetRoot.name}' に変更はありませんでした。");
                    }
                }
                else
                {
                    // ===== 通常のPrefabアセットの場合 =====
                    GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);

                    foreach (Renderer r in renderers)
                    {
                        Material[] sharedMats = r.sharedMaterials;
                        bool rChanged = false;

                        for (int j = 0; j < sharedMats.Length; j++)
                        {
                            Material m = sharedMats[j];
                            string currentMatName = m != null ? m.name : "";

                            Material foundMat = FindMaterial(currentMatName, assetRoot.name, availableMaterials, r.gameObject.name);

                            if (foundMat != null && foundMat != m)
                            {
                                if (showDebugLog)
                                    Debug.Log($"[Prefab更新] {assetRoot.name} ({r.name}) の '{currentMatName}' を '{foundMat.name}' に変更しました。");

                                sharedMats[j] = foundMat;
                                rChanged = true;
                                changed = true;
                            }
                        }

                        if (rChanged) r.sharedMaterials = sharedMats;
                    }

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
                        successCount++;
                    }
                    else if (showDebugLog)
                    {
                        Debug.LogWarning($"[スキップ] Prefab '{assetRoot.name}' に変更はありませんでした。");
                    }

                    PrefabUtility.UnloadPrefabContents(instance);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完了", $"{modelGuids.Length}個のモデルを確認し、\n{successCount}個のモデルを更新しました。\n詳細はConsoleを確認してください。", "OK");
    }

    /// <summary>
    /// 条件に従って最適なマテリアルを双方向部分一致で検索して返す
    /// </summary>
    private Material FindMaterial(string currentMatName, string modelName, List<Material> availableMaterials, string gameObjectName = "")
    {
        string searchMatName = "";

        // --- 1. 元マテリアル名で検索 ---
        if (!string.IsNullOrEmpty(currentMatName) && currentMatName != "Default-Material" && currentMatName != "No Name")
        {
            searchMatName = currentMatName.Replace(" (Instance)", "").Trim();

            // ユーザーが指定した不要な文字列を除外
            if (!string.IsNullOrEmpty(ignoreStringsInput))
            {
                string[] ignores = ignoreStringsInput.Split(',');
                foreach (string ignoreStr in ignores)
                {
                    string trimIgnore = ignoreStr.Trim();
                    if (!string.IsNullOrEmpty(trimIgnore))
                    {
                        searchMatName = searchMatName.Replace(trimIgnore, "");
                    }
                }
            }
            searchMatName = searchMatName.Trim();

            foreach (Material mat in availableMaterials)
            {
                if (mat.name.IndexOf(searchMatName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    searchMatName.IndexOf(mat.name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mat;
                }
            }
        }

        // --- 2. モデル名 (親アセット名) で検索 ---
        if (!string.IsNullOrEmpty(modelName))
        {
            foreach (Material mat in availableMaterials)
            {
                if (mat.name.IndexOf(modelName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    modelName.IndexOf(mat.name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mat;
                }
            }
        }

        // --- 3. モデル内のRendererがついているGameObject名で検索 ---
        if (!string.IsNullOrEmpty(gameObjectName))
        {
            foreach (Material mat in availableMaterials)
            {
                if (mat.name.IndexOf(gameObjectName, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    gameObjectName.IndexOf(mat.name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mat;
                }
            }
        }

        return null;
    }
}