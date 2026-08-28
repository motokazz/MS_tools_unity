using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PrefabReplacer : EditorWindow
{
    private string targetFolderPath = "Assets";
    private string prefixText = "";
    private string suffixText = "";
    private Object go;

    // 追加子オブジェクト・コンポーネントの保持フラグ
    private bool keepAddedChildren = true;
    private bool keepAddedComponents = true;

    [MenuItem("MS_Tools/Assets/PrefabReplacer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabReplacer>("Prefab Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab 差し替えツール (子要素・複数コリジョン維持版)", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // 対象フォルダUI
        GUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("対象フォルダ", targetFolderPath);
        if (GUILayout.Button("参照", GUILayout.Width(50)))
        {
            string path = EditorUtility.OpenFolderPanel("対象フォルダを選択", targetFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    targetFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("プロジェクト内のフォルダ(Assets以下)を選択してください。");
                }
            }
        }
        GUILayout.EndHorizontal();

        prefixText = EditorGUILayout.TextField("Prefix", prefixText);
        suffixText = EditorGUILayout.TextField("Suffix", suffixText);

        GUILayout.Space(15);

        GUI.enabled = Selection.gameObjects.Length > 0 && !string.IsNullOrEmpty(targetFolderPath);
        if (GUILayout.Button("実行", GUILayout.Height(30)))
        {
            ExecuteReplacement();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // ▼ 追加：保持設定のトグル群
        keepAddedChildren = EditorGUILayout.Toggle("追加子オブジェクトを維持", keepAddedChildren);
        keepAddedComponents = EditorGUILayout.Toggle("追加コンポーネントを維持", keepAddedComponents);

        if (keepAddedChildren || keepAddedComponents)
        {
            EditorGUILayout.HelpBox("チェックが入っている項目は、差し替え後のオブジェクトに自動で移行・維持されます。\n（※既存コンポーネントの数値変更などは常に引き継がれます）", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("追加された子オブジェクトやコンポーネントは維持されず、クリーンな状態で差し替えられます。", MessageType.Warning);
        }

        GUILayout.Space(10);
        go = EditorGUILayout.ObjectField("指定Prefab", go, typeof(GameObject), false);

        GUI.enabled = Selection.gameObjects.Length > 0 && go != null;
        if (GUILayout.Button("実行 (直接指定)", GUILayout.Height(30)))
        {
            ExecuteReplacementEasy();
        }
        GUI.enabled = true;
    }

    private void ExecuteReplacement()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0) return;

        if (!AssetDatabase.IsValidFolder(targetFolderPath))
        {
            Debug.LogError($"[PrefabReplacer] 対象フォルダが存在しません: {targetFolderPath}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { targetFolderPath });
        List<GameObject> folderPrefabs = new List<GameObject>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) folderPrefabs.Add(prefab);
        }

        int successCount = 0;
        int failCount = 0;
        List<GameObject> newSelection = new List<GameObject>();

        foreach (GameObject targetObj in selectedObjects)
        {
            GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(targetObj);

            if (sourcePrefab == null)
            {
                Debug.LogWarning($"[PrefabReplacer] <color=yellow>スキップ</color>: '{targetObj.name}' はプレファブのインスタンスではないため、リンク先名を取得できません。");
                failCount++;
                continue;
            }

            string baseName = sourcePrefab.name;
            string searchName = $"{prefixText}{baseName}{suffixText}";

            GameObject replacementPrefab = folderPrefabs.Find(p => p.name.Contains(searchName));

            if (replacementPrefab == null)
            {
                Debug.LogWarning($"[PrefabReplacer] <color=orange>失敗</color>: '{targetObj.name}' (リンク先名: {baseName}) に一致するPrefabが見つかりません。検索対象: {searchName}");
                failCount++;
                continue;
            }

            string replacementPrefabName = replacementPrefab.name;
            string targetObjName = targetObj.name;
            newSelection.Add(ExecuteReplacementCore(replacementPrefab, targetObj));
            Debug.Log($"[PrefabReplacer] <color=lime>成功</color>: '{targetObjName}' -> '{replacementPrefabName}' (差し替え完了)");
            successCount++;
        }

        if (newSelection.Count > 0) Selection.objects = newSelection.ToArray();
        Debug.Log($"[PrefabReplacer] <b>作業完了</b> - 成功: {successCount} / 失敗: {failCount}");
    }

    private void ExecuteReplacementEasy()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0 || go == null) return;

        int successCount = 0;
        int failCount = 0;
        List<GameObject> newSelection = new List<GameObject>();

        foreach (GameObject targetObj in selectedObjects)
        {
            if (targetObj == null) continue;

            GameObject replacementPrefab = (GameObject)go;

            string replacementPrefabName = replacementPrefab.name;
            string targetObjName = targetObj.name;
            newSelection.Add(ExecuteReplacementCore(replacementPrefab, targetObj));
            Debug.Log($"[PrefabReplacer] <color=lime>成功</color>: '{targetObjName}' -> '{replacementPrefabName}' (直接指定で差し替えました)");
            successCount++;
        }

        if (newSelection.Count > 0) Selection.objects = newSelection.ToArray();
        Debug.Log($"[PrefabReplacer] <b>作業完了</b> - 成功: {successCount} / 失敗: {failCount}");
    }

    private GameObject ExecuteReplacementCore(GameObject replacementPrefab, GameObject targetObj)
    {
        GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
        Undo.RegisterCreatedObjectUndo(newInstance, "Replace Prefab");

        newInstance.transform.SetParent(targetObj.transform.parent);
        newInstance.transform.localPosition = targetObj.transform.localPosition;
        newInstance.transform.localRotation = targetObj.transform.localRotation;
        newInstance.transform.localScale = targetObj.transform.localScale;
        newInstance.transform.SetSiblingIndex(targetObj.transform.GetSiblingIndex());

        if (keepAddedChildren)
        {
            MoveAddedChildren(targetObj, newInstance);
        }

        CopyComponents(targetObj, newInstance);

        Undo.DestroyObjectImmediate(targetObj);

        return newInstance;
    }

    private void MoveAddedChildren(GameObject source, GameObject destination)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in source.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            if (PrefabUtility.IsAddedGameObjectOverride(child.gameObject) || !PrefabUtility.IsPartOfAnyPrefab(child.gameObject))
            {
                Undo.SetTransformParent(child, destination.transform, "Move Added Child");
            }
        }
    }

    private void CopyComponents(GameObject source, GameObject destination)
    {
        Component[] sourceComponents = source.GetComponents<Component>();

        Dictionary<System.Type, int> componentCounts = new Dictionary<System.Type, int>();

        foreach (Component sourceComp in sourceComponents)
        {
            if (sourceComp == null || sourceComp is Transform || sourceComp is LODGroup) continue;

            System.Type type = sourceComp.GetType();

            if (!componentCounts.ContainsKey(type))
            {
                componentCounts[type] = 0;
            }

            int currentIndex = componentCounts[type];

            Component[] destComponents = destination.GetComponents(type);

            // 差し替え先にも同型のコンポーネントが存在する場合（値の引き継ぎ）
            if (currentIndex < destComponents.Length)
            {
                Component destComp = destComponents[currentIndex];
                EditorUtility.CopySerialized(sourceComp, destComp);
            }
            // 差し替え元にしか存在しない（シーン等で追加された）コンポーネントの場合
            else
            {
                // ▼ 変更：追加コンポーネントの維持がONの場合のみコピーする
                if (keepAddedComponents)
                {
                    Component destComp = Undo.AddComponent(destination, type);
                    EditorUtility.CopySerialized(sourceComp, destComp);
                }
            }

            componentCounts[type]++;
        }
    }
}