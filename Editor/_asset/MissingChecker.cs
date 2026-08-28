using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MS_MissingFinder : EditorWindow
{
    private Vector2 scroll;
    private List<GameObject> targets = new List<GameObject>();

    private Vector2 resultScroll;

    private class MissingInfo
    {
        public GameObject gameObject;
        public Component component;
        public string componentName;
        public string propertyName;
        public string path;
        public bool isMissingComponent;
    }

    private List<MissingInfo> results = new List<MissingInfo>();

    [MenuItem("MS_Tools/Assets/Missing Finder")]
    public static void ShowWindow()
    {
        GetWindow<MS_MissingFinder>("Missing Finder");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Missing Finder Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "選択したオブジェクトとその子孫からMissingを検出します。\n\n" +
            "・Missing Component\n" +
            "・Missing Reference\n\n" +
            "クリックで該当オブジェクトにジャンプできます",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("選択中オブジェクトを登録"))
        {
            targets.Clear();
            targets.AddRange(Selection.gameObjects);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("対象オブジェクト", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(80));
        foreach (var t in targets)
        {
            EditorGUILayout.ObjectField(t, typeof(GameObject), true);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("実行（Missingチェック）"))
        {
            FindMissing();
        }

        EditorGUILayout.Space();

        DrawResults();
    }

    private void DrawResults()
    {
        EditorGUILayout.LabelField($"検出結果: {results.Count} 件", EditorStyles.boldLabel);

        resultScroll = EditorGUILayout.BeginScrollView(resultScroll);

        foreach (var r in results)
        {
            EditorGUILayout.BeginHorizontal("box");

            string label = r.isMissingComponent
                ? $"[Missing Component] {r.path}"
                : $"[Missing Reference] {r.path} / {r.componentName} : {r.propertyName}";

            if (GUILayout.Button(label, GUILayout.ExpandWidth(true)))
            {
                SelectObject(r);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void SelectObject(MissingInfo info)
    {
        if (info.gameObject == null) return;

        // Hierarchyで選択
        Selection.activeGameObject = info.gameObject;

        // Ping（ハイライト）
        EditorGUIUtility.PingObject(info.gameObject);

        // コンポーネントも特定
        if (!info.isMissingComponent && info.component != null)
        {
            EditorGUIUtility.PingObject(info.component);
        }
    }

    private void FindMissing()
    {
        results.Clear();

        if (targets.Count == 0)
        {
            Debug.LogWarning("対象オブジェクトが登録されていません");
            return;
        }

        foreach (var root in targets)
        {
            if (root == null) continue;

            var transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (var t in transforms)
            {
                var components = t.GetComponents<Component>();

                foreach (var comp in components)
                {
                    // Missing Component
                    if (comp == null)
                    {
                        results.Add(new MissingInfo
                        {
                            gameObject = t.gameObject,
                            component = null,
                            componentName = "Missing Component",
                            propertyName = "",
                            path = GetFullPath(t),
                            isMissingComponent = true
                        });

                        Debug.LogWarning($"[Missing Component] {GetFullPath(t)}", t);
                        continue;
                    }

                    // Missing Reference
                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty prop = so.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (prop.objectReferenceValue == null &&
                                prop.objectReferenceInstanceIDValue != 0)
                            {
                                var info = new MissingInfo
                                {
                                    gameObject = t.gameObject,
                                    component = comp,
                                    componentName = comp.GetType().Name,
                                    propertyName = prop.name,
                                    path = GetFullPath(t),
                                    isMissingComponent = false
                                };

                                results.Add(info);

                                Debug.LogWarning(
                                    $"[Missing Reference] {info.path} / {info.componentName} : {info.propertyName}",
                                    t
                                );
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"Missingチェック完了: {results.Count} 件検出");
    }

    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}