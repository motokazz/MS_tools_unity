using UnityEditor;
using UnityEngine;

public class SetChildrenStatic:EditorWindow
{
    [MenuItem("MS_Tools/Level/Set Children Static")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SetChildrenStatic));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("SetStaticToAllChildren");
        GUILayout.BeginVertical();


        // SelectSourcePrefabs
        if (GUILayout.Button("SetStaticToAllChildren", GUILayout.Width(200), GUILayout.Height(30)))
        {
            ApplyStaticFlagsToChildren();
        }


        GUILayout.EndVertical();
    }


    static void ApplyStaticFlagsToChildren()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is GameObject go)
            {
                string assetPath = AssetDatabase.GetAssetPath(go);

                if (!string.IsNullOrEmpty(assetPath)) // Project上のPrefabアセット
                {
                    ApplyToPrefabAsset(assetPath);
                }
                else if (go.scene.IsValid()) // Hierarchy上のGameObject
                {
                    ApplyToHierarchyInstance(go);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Static設定を子に適用しました。");
    }

    static void ApplyToHierarchyInstance(GameObject parent)
    {
        StaticEditorFlags parentFlags = GameObjectUtility.GetStaticEditorFlags(parent);
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child == parent.transform) continue;
            GameObjectUtility.SetStaticEditorFlags(child.gameObject, parentFlags);
        }
    }

    static void ApplyToPrefabAsset(string assetPath)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
        if (prefabRoot == null)
        {
            Debug.LogWarning("Prefabの読み込みに失敗しました: " + assetPath);
            return;
        }

        StaticEditorFlags parentFlags = GameObjectUtility.GetStaticEditorFlags(prefabRoot);
        foreach (Transform child in prefabRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child == prefabRoot.transform) continue;
            GameObjectUtility.SetStaticEditorFlags(child.gameObject, parentFlags);
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
}
