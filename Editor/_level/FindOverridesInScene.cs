using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class FindNonTransformOverrides : EditorWindow
{
    [MenuItem("MS_Tools/Level/Find Non-Transform Prefab Overrides")]
    public static void ShowWindow()
    {
        GetWindow<FindNonTransformOverrides>("Override Checker");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find Non-Transform Prefab Overrides"))
        {
            FindOverrides();
        }
    }

    private void FindOverrides()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(obj))
                continue;

            // オーバーライド情報を正しく取得
            var overrides = PrefabUtility.GetObjectOverrides(obj, false);
            var propertyOverrides = PrefabUtility.GetPropertyModifications(obj);

            // オブジェクト自体にオーバーライドがある場合（例：Componentの追加など）
            bool hasObjectOverride = overrides != null && overrides.Count > 0;

            // プロパティオーバーライドがあるか（Transform以外に）
            bool hasNonTransformPropertyOverride = false;
            if (propertyOverrides != null)
            {
                foreach (var mod in propertyOverrides)
                {
                    if (mod?.target != null && !(mod.target is Transform))
                    {
                        hasNonTransformPropertyOverride = true;
                        break;
                    }
                }
            }

            if (hasObjectOverride || hasNonTransformPropertyOverride)
            {
                Debug.Log($"Non-Transform override: {obj.name}", obj);
                count++;
            }
        }

        Debug.Log($"✅ Total objects with non-Transform overrides: {count}");
    }
}
