using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 
/// </summary>

public class SelectPrefabSource : EditorWindow
{
    [MenuItem("MS_Tools/Level/MS_SelectPrefabSource")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SelectPrefabSource));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("MS_selection");
        GUILayout.BeginVertical();


        // SelectSourcePrefabs
        if (GUILayout.Button("Selection", GUILayout.Width(200), GUILayout.Height(30)))
        {
            SelectSourcePrefabs();
        }
        
        // SelectSourcePrefabs
        if (GUILayout.Button("SelectionChildlens", GUILayout.Width(200), GUILayout.Height(30)))
        {
            SelectSourceChildlens();
        }
        
        // SelectSourcePrefabs
        if (GUILayout.Button("SelectionPrefabRoot", GUILayout.Width(200), GUILayout.Height(30)))
        {
            SelectRootSource();
        }


        GUILayout.EndVertical();
    }


    // 選択中Prefabのソース選択
    static void SelectSourcePrefabs()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        SelectSourcePrefabs(selectedObjects);
    }

    // 子孫対象
    static void SelectSourceChildlens()
    {
        GameObject[] selectedObjects = ListPrefabsUnderSelection();
        SelectSourcePrefabs(selectedObjects);
    }
    //　RootPrefab対象
    static void SelectRootSource()
    {
        GameObject[] selectedObjects = SelectRootOfPrefab();
        SelectSourcePrefabs(selectedObjects);
    }


    // 本体
    private static void SelectSourcePrefabs(GameObject[] selectedObjects)
    {

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていません。");
            return;
        }

        HashSet<Object> prefabAssets = new HashSet<Object>();

        foreach (var obj in selectedObjects)
        {


            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (prefab != null)
            {
                prefabAssets.Add(prefab);
            }
            else
            {
                Debug.LogWarning($"'{obj.name}' はPrefabインスタンスではありません。");
            }
        }

        if (prefabAssets.Count > 0)
        {
            Selection.objects = new List<Object>(prefabAssets).ToArray();
            EditorGUIUtility.PingObject(Selection.objects[0]);
        }
        else
        {
            Debug.LogWarning("選択されたオブジェクトの中にPrefabインスタンスが見つかりませんでした。");
        }
    }




    //　Gameojectルートのリスト作成
    private static GameObject[] SelectRootOfPrefab()
    {
        List<GameObject> roots = new List<GameObject>();

        //GameObject[] selectedObjects = Selection.gameObjects;
        GameObject[] selectedObjects = ListPrefabsUnderSelection();

        foreach (GameObject go in selectedObjects)
        {
            //root選択
            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);

            if (root != null)
            {
                if (!roots.Contains(root))
                {
                    roots.Add(root);
                }
            }
            else
            {
                roots.Add(go);
            }
        }

        return roots.ToArray();

    }
    
    // Gameobject子孫検索
    static GameObject[] ListPrefabsUnderSelection()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("何も選択されていません。");
            return null;
        }

        List<GameObject> prefabInstances = new List<GameObject>();

        TraverseHierarchy(selected.transform, go =>
        {
            var status = PrefabUtility.GetPrefabInstanceStatus(go);
            if (status == PrefabInstanceStatus.Connected)
            {
                prefabInstances.Add(go);
            }
        });

        if (prefabInstances.Count == 0)
        {
            Debug.Log("子孫にPrefabインスタンスは見つかりませんでした。");
        }
        else
        {
            Debug.Log($"Prefabインスタンス一覧（{prefabInstances.Count}個）:");
            /*
            foreach (var go in prefabInstances)
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
                string name = prefabAsset != null ? prefabAsset.name : "(Unknown)";
                Debug.Log($"- {go.name} (元Prefab: {name})", go);
            }
            */
        }
        return prefabInstances.ToArray();
    }

    static void TraverseHierarchy(Transform root, System.Action<GameObject> action)
    {
        foreach (Transform child in root)
        {
            action(child.gameObject);
            TraverseHierarchy(child, action);
        }
    }


}
