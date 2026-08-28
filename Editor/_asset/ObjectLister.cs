using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MSTools
{
    public class ObjectListerWindow : EditorWindow
    {
        private class ListedItem
        {
            public Object targetObject;
            public Object referencer;
            public string name;
            public string projectPath;
            public ObjectTypeFilter type;
            public bool hasMissing;
        }

        [System.Flags]
        public enum ObjectTypeFilter
        {
            None = 0,
            GameObject = 1 << 0,
            Mesh = 1 << 1,
            Material = 1 << 2,
            Texture = 1 << 3,
            AudioClip = 1 << 4,
            AnimationClip = 1 << 5,
            ScriptableObject = 1 << 6,
            Other = 1 << 7,
            All = ~0
        }

        // ★追加: ソート用の列挙型と状態保持変数
        private enum SortColumn { Status, Type, Name, Referencer, Path }
        private SortColumn currentSortColumn = SortColumn.Name;
        private bool sortAscending = true;

        private List<ListedItem> allItems = new List<ListedItem>();
        private List<ListedItem> filteredItems = new List<ListedItem>();

        private Vector2 scrollPosList;
        private string searchString = "";
        private ObjectTypeFilter typeFilter = ObjectTypeFilter.All;
        private bool showOnlyMissing = false;

        [MenuItem("MS_Tools/Assets/Object Lister")]
        public static void ShowWindow() => GetWindow<ObjectListerWindow>("Object Lister");

        // 選択が変更されたらウィンドウを再描画して対象の件数を更新する
        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnGUI()
        {
            DrawHelpBox();
            EditorGUILayout.Space();
            DrawTargetSection();
            EditorGUILayout.Space();
            DrawExecuteButton();
            EditorGUILayout.Space();
            DrawFilterSection();
            EditorGUILayout.Space();
            DrawListSection();
        }

        private void DrawHelpBox()
        {
            EditorGUILayout.HelpBox(
                "【使い方】\n" +
                "1. ProjectやHierarchyで調査したいオブジェクトを選択します（複数可）。\n" +
                "2. 「リスト作成」ボタンで、含まれるアセットを抽出します。\n" +
                "3. ヘッダーをクリックすると並び替えができます。\n" + // ★変更
                "4. 名前をクリックするとアセット自体を、[参照元]ボタンを押すと「そのアセットを使っているオブジェクト」を選択します。",
                MessageType.Info);
        }

        private void DrawTargetSection()
        {
            GUILayout.Label("◆ 検索対象 (現在選択中のオブジェクト)", EditorStyles.boldLabel);

            Object[] selectedObjects = Selection.objects;

            if (selectedObjects.Length == 0)
            {
                EditorGUILayout.HelpBox("対象が選択されていません。ProjectやHierarchyでオブジェクトを選択してください。", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"現在 {selectedObjects.Length} 件のオブジェクトが選択されています。", MessageType.Info);
            }
        }

        private void DrawExecuteButton()
        {
            EditorGUI.BeginDisabledGroup(Selection.objects.Length == 0);
            GUI.backgroundColor = new Color(0.6f, 1.0f, 0.6f);
            if (GUILayout.Button("リスト作成（選択中のオブジェクトから抽出）", GUILayout.Height(30)))
            {
                GenerateList();
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
        }

        private void DrawFilterSection()
        {
            GUILayout.Label("◆ フィルタリング", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            typeFilter = (ObjectTypeFilter)EditorGUILayout.EnumFlagsField("種類", typeFilter);
            searchString = EditorGUILayout.TextField("パス/名前で絞り込み", searchString);
            showOnlyMissing = EditorGUILayout.Toggle("Missingのみ表示", showOnlyMissing);
            if (EditorGUI.EndChangeCheck()) ApplyFilter();
        }

        private void DrawListSection()
        {
            GUILayout.Label($"◆ 抽出結果 ({filteredItems.Count} 件 / 全 {allItems.Count} 件)", EditorStyles.boldLabel);
            if (filteredItems.Count == 0)
            {
                EditorGUILayout.HelpBox("条件に一致するオブジェクトはありません。", MessageType.None);
                return;
            }

            // ★変更: ヘッダーをボタンにしてソート処理を呼び出せるように改修
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(GetSortHeader("状態", SortColumn.Status), EditorStyles.toolbarButton, GUILayout.Width(50))) SortBy(SortColumn.Status);
            if (GUILayout.Button(GetSortHeader("種類", SortColumn.Type), EditorStyles.toolbarButton, GUILayout.Width(80))) SortBy(SortColumn.Type);
            if (GUILayout.Button(GetSortHeader("アセット名", SortColumn.Name), EditorStyles.toolbarButton, GUILayout.Width(180))) SortBy(SortColumn.Name);
            if (GUILayout.Button(GetSortHeader("参照元", SortColumn.Referencer), EditorStyles.toolbarButton, GUILayout.Width(60))) SortBy(SortColumn.Referencer);
            if (GUILayout.Button(GetSortHeader("Projectパス", SortColumn.Path), EditorStyles.toolbarButton, GUILayout.ExpandWidth(true))) SortBy(SortColumn.Path);
            EditorGUILayout.EndHorizontal();

            scrollPosList = EditorGUILayout.BeginScrollView(scrollPosList);
            foreach (var item in filteredItems)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (item.hasMissing)
                {
                    GUI.contentColor = Color.red;
                    GUILayout.Label("Missing", GUILayout.Width(50));
                    GUI.contentColor = Color.white;
                }
                else GUILayout.Label("OK", GUILayout.Width(50));

                GUILayout.Label(item.type.ToString(), GUILayout.Width(80));

                if (GUILayout.Button(item.name, EditorStyles.linkLabel, GUILayout.Width(180)))
                {
                    Selection.activeObject = item.targetObject;
                    EditorGUIUtility.PingObject(item.targetObject);
                }

                if (GUILayout.Button("参照元", GUILayout.Width(60)))
                {
                    if (item.referencer != null)
                    {
                        Selection.activeObject = item.referencer;
                        EditorGUIUtility.PingObject(item.referencer);
                    }
                    else
                    {
                        Debug.LogWarning("直接の参照元オブジェクトが見つかりません。（アセット単体として追加された可能性があります）");
                    }
                }

                GUILayout.Label(item.projectPath, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        // ★追加: ソートボタンのヘッダーテキストを生成（矢印付き）
        private string GetSortHeader(string label, SortColumn column)
        {
            if (currentSortColumn == column)
                return label + (sortAscending ? " ▲" : " ▼");
            return label;
        }

        // ★追加: ソートの実行（同じ列なら昇順降順切り替え、違う列なら昇順でその列に）
        private void SortBy(SortColumn column)
        {
            if (currentSortColumn == column)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = column;
                sortAscending = true;
            }
            ApplySort();
        }

        // ★追加: フィルタリングされたリストを実際に並び替える処理
        private void ApplySort()
        {
            if (filteredItems == null || filteredItems.Count == 0) return;

            switch (currentSortColumn)
            {
                case SortColumn.Status:
                    filteredItems = sortAscending
                        ? filteredItems.OrderBy(i => !i.hasMissing).ThenBy(i => i.name).ToList() // Missing(false)を上に
                        : filteredItems.OrderByDescending(i => !i.hasMissing).ThenBy(i => i.name).ToList();
                    break;
                case SortColumn.Type:
                    filteredItems = sortAscending
                        ? filteredItems.OrderBy(i => i.type.ToString()).ThenBy(i => i.name).ToList()
                        : filteredItems.OrderByDescending(i => i.type.ToString()).ThenBy(i => i.name).ToList();
                    break;
                case SortColumn.Name:
                    filteredItems = sortAscending
                        ? filteredItems.OrderBy(i => i.name).ToList()
                        : filteredItems.OrderByDescending(i => i.name).ToList();
                    break;
                case SortColumn.Referencer:
                    filteredItems = sortAscending
                        ? filteredItems.OrderBy(i => i.referencer != null ? i.referencer.name : "").ToList()
                        : filteredItems.OrderByDescending(i => i.referencer != null ? i.referencer.name : "").ToList();
                    break;
                case SortColumn.Path:
                    filteredItems = sortAscending
                        ? filteredItems.OrderBy(i => i.projectPath).ToList()
                        : filteredItems.OrderByDescending(i => i.projectPath).ToList();
                    break;
            }
        }

        private void GenerateList()
        {
            allItems.Clear();
            HashSet<Object> processed = new HashSet<Object>();

            foreach (var root in Selection.objects)
            {
                if (root == null) continue;

                string rootPath = AssetDatabase.GetAssetPath(root);

                if (AssetDatabase.IsValidFolder(rootPath))
                {
                    string[] guids = AssetDatabase.FindAssets("", new[] { rootPath });
                    foreach (var guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

                        if (mainAsset is GameObject go)
                        {
                            ParseHierarchy(go, processed);
                        }
                        else if (mainAsset != null)
                        {
                            Object[] deps = EditorUtility.CollectDependencies(new[] { mainAsset });
                            foreach (var dep in deps)
                            {
                                if (dep is GameObject || dep is Component) continue;
                                AddToList(dep, processed, mainAsset);
                            }
                            AddToList(mainAsset, processed, mainAsset);
                        }
                    }
                }
                else if (root is GameObject go)
                {
                    ParseHierarchy(go, processed);
                }
                else
                {
                    Object[] deps = EditorUtility.CollectDependencies(new[] { root });
                    foreach (var dep in deps)
                    {
                        if (dep is GameObject || dep is Component) continue;
                        AddToList(dep, processed, root);
                    }
                    AddToList(root, processed, root);
                }
            }
            ApplyFilter();
        }

        private void ParseHierarchy(GameObject rootGo, HashSet<Object> processed)
        {
            Transform[] children = rootGo.GetComponentsInChildren<Transform>(true);
            foreach (var t in children)
            {
                GameObject go = t.gameObject;

                AddToList(go, processed, go);

                Component[] components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null || comp is Transform) continue;

                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            Object refObj = sp.objectReferenceValue;

                            if (refObj != null && !(refObj is GameObject) && !(refObj is Component))
                            {
                                AddToList(refObj, processed, go);

                                Object[] deepDeps = EditorUtility.CollectDependencies(new Object[] { refObj });
                                foreach (var dep in deepDeps)
                                {
                                    if (dep == null || dep is GameObject || dep is Component) continue;
                                    AddToList(dep, processed, refObj);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddToList(Object obj, HashSet<Object> processed, Object referencer)
        {
            if (obj == null || processed.Contains(obj) || obj is Component) return;

            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
            {
                if (obj is GameObject) path = "Scene Object";
                else return;
            }

            processed.Add(obj);
            allItems.Add(new ListedItem
            {
                targetObject = obj,
                referencer = referencer,
                name = obj.name,
                projectPath = path,
                type = GetObjectType(obj),
                hasMissing = CheckMissing(obj)
            });
        }

        private void ApplyFilter()
        {
            filteredItems = allItems.Where(item => {
                if (showOnlyMissing && !item.hasMissing) return false;
                if ((typeFilter & item.type) == 0) return false;
                if (!string.IsNullOrEmpty(searchString))
                {
                    string lowerSearch = searchString.ToLower();
                    if (!item.name.ToLower().Contains(lowerSearch) && !item.projectPath.ToLower().Contains(lowerSearch)) return false;
                }
                return true;
            }).ToList();

            ApplySort(); // ★変更: フィルタリング後に必ずソートを適用する
        }

        private ObjectTypeFilter GetObjectType(Object obj)
        {
            if (obj is GameObject) return ObjectTypeFilter.GameObject;
            if (obj is Mesh) return ObjectTypeFilter.Mesh;
            if (obj is Material) return ObjectTypeFilter.Material;
            if (obj is Texture) return ObjectTypeFilter.Texture;
            if (obj is AudioClip) return ObjectTypeFilter.AudioClip;
            if (obj is AnimationClip) return ObjectTypeFilter.AnimationClip;
            if (obj is ScriptableObject) return ObjectTypeFilter.ScriptableObject;
            return ObjectTypeFilter.Other;
        }

        private bool CheckMissing(Object obj)
        {
            if (obj is GameObject go)
            {
                if (go.GetComponents<Component>().Any(c => c == null)) return true;
                if (PrefabUtility.IsPrefabAssetMissing(go)) return true;
            }
            return false;
        }
    }
}