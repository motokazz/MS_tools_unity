using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class ComponentRemoverWindow : EditorWindow
{
    private Dictionary<Type, bool> componentTypes = new Dictionary<Type, bool>();
    private Vector2 scrollPos;

    [MenuItem("MS_Tools/Remove Components from Children")]
    public static void ShowWindow()
    {
        GetWindow<ComponentRemoverWindow>("Component Remover");
    }

    private void OnSelectionChange()
    {
        RefreshComponentList();
        Repaint();
    }

    private void OnFocus()
    {
        RefreshComponentList();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("選択オブジェクトの子孫に使われているコンポーネント一覧", EditorStyles.boldLabel);

        if (componentTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("子孫に有効なコンポーネントが見つかりません。", MessageType.Info);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var key in componentTypes.Keys.ToList())
        {
            componentTypes[key] = EditorGUILayout.ToggleLeft(key.Name, componentTypes[key]);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("✅ 選択したコンポーネントを削除"))
        {
            RemoveSelectedComponents();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("🗑 Transformのみかつ子がいない空オブジェクトを削除"))
        {
            RemoveEmptyLeafGameObjects();
        }
    }

    void RefreshComponentList()
    {
        componentTypes.Clear();

        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0) return;

        var allComponents = new HashSet<Type>();

        foreach (GameObject go in selected)
        {
            Component[] comps = go.GetComponentsInChildren<Component>(true);

            foreach (var comp in comps)
            {
                if (comp == null) continue;

                Type t = comp.GetType();

                if (t == typeof(Transform)) continue;

                allComponents.Add(t);
            }
        }

        foreach (var type in allComponents)
        {
            componentTypes[type] = false;
        }
    }

    void RemoveSelectedComponents()
    {
        int removedCount = 0;
        var typesToRemove = componentTypes.Where(kv => kv.Value).Select(kv => kv.Key).ToList();

        if (typesToRemove.Count == 0)
        {
            Debug.LogWarning("削除対象のコンポーネントが選ばれていません。");
            return;
        }

        GameObject[] selected = Selection.gameObjects;

        foreach (GameObject go in selected)
        {
            Component[] comps = go.GetComponentsInChildren<Component>(true);

            foreach (var comp in comps)
            {
                if (comp == null) continue;

                Type t = comp.GetType();
                if (typesToRemove.Contains(t))
                {
                    Undo.DestroyObjectImmediate(comp);
                    removedCount++;
                }
            }
        }

        Debug.Log($"{removedCount} 個のコンポーネントを削除しました。");
        RefreshComponentList(); // 再読み込み
    }

    void RemoveEmptyLeafGameObjects()
    {
        int removed = 0;
        GameObject[] selected = Selection.gameObjects;

        var allChildren = new List<GameObject>();

        foreach (GameObject go in selected)
        {
            allChildren.AddRange(go.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
        }

        foreach (var go in allChildren)
        {
            if (go == null) continue;

            Component[] comps = go.GetComponents<Component>();

            // Transformのみで、かつ子がいない
            if (comps.Length == 1 && comps[0] is Transform && go.transform.childCount == 0)
            {
                Undo.DestroyObjectImmediate(go);
                removed++;
            }
        }

        Debug.Log($"Transformのみかつ子なしの空オブジェクトを {removed} 個削除しました。");
        RefreshComponentList(); // 再読み込み
    }
}
