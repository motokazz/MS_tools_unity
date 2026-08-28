using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// シーンに直置きされたFBXを選択する
/// </summary>


public class FbxInSceneFinder:EditorWindow
{
    [MenuItem("MS_Tools/Level/MS_FindFbxInScene")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FbxInSceneFinder));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("MS_FindFbxInScene");

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindFbxInScene","インポートしたFbxを直接置いてるオブジェクト"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindFbxInScene();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindMissingAssetInScene", "参照先がMissingになってるオブジェクト"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindMissingAssetInScene();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindNotAPrefabInScene", "Prefabではないオブジェクト"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindNotAPrefabInScene();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindPrefabVariantInScene", "PrefabVariant"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindPrefabVariantInScene();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindRegularInScene", "AssetTypeがRegularのオブジェクト"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindRegularInScene();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button(new GUIContent("FindAddedObjects", "追加オブジェクト（アイコンに＋がついている奴）"), GUILayout.Width(200), GUILayout.Height(30)))
        {
            FindAddedObjects();
        }


        GUILayout.EndVertical();
    }





    public static void FindFbxInScene()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> fbxInstances = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Model &&
                PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(obj));
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    fbxInstances.Add(obj);
                }
            }
        }

        Debug.Log($"Found {fbxInstances.Count} FBX instance(s) in the scene:");

        Selection.objects = fbxInstances.ToArray();
        /*
        foreach (var go in fbxInstances)
        {
            string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go));
            Debug.Log($"- {go.name} (Path: {path})", go);
        }
        */
    }

    public static void FindPrefabVariantInScene()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> fbxInstances = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Variant &&
                PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                fbxInstances.Add(obj);
            }
        }

        Selection.objects = fbxInstances.ToArray();
    }

    public static void FindNotAPrefabInScene()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> fbxInstances = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab &&
                PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                fbxInstances.Add(obj);
            }
        }

        Selection.objects = fbxInstances.ToArray();
    }

    public static void FindMissingAssetInScene()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        List<GameObject> fbxInstances = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.MissingAsset &&
                PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                fbxInstances.Add(obj);
            }
        }

        Selection.objects = fbxInstances.ToArray();
    }

    public static void FindRegularInScene()
    {
        //GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        List<GameObject> fbxInstances = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Regular &&
                PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
            {
                fbxInstances.Add(obj);
            }
        }

        Selection.objects = fbxInstances.ToArray();
    }

    public static void FindAddedObjects()
    {
        List<GameObject> addedObjects = new List<GameObject>();
        //GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true); // 非アクティブも含む
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // プレハブインスタンスの中にあるか？
            var outerPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
            if (outerPrefab == null) continue;

            // 元のプレハブに対応するオブジェクトが存在しない → 追加されたオブジェクト
            var sourceObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (sourceObj == null)
            {
                addedObjects.Add(obj);
            }
        }

        if (addedObjects.Count > 0)
        {
            Selection.objects = addedObjects.ToArray();
            Debug.Log($"Found and selected {addedObjects.Count} added object(s) in prefab instances.");
        }
        else
        {
            Selection.objects = null;
            Debug.Log("No added objects found in prefab instances.");
        }
    }
}
