using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshVertexCheckerWindow : EditorWindow
{
    private List<MeshInfo> meshInfos = new List<MeshInfo>();
    private int totalVertices = 0;
    private Vector2 scrollPosition;
    private GUIStyle linkStyle;

    private enum SortType { None, GameObjectName, MeshName, VertexCount, BoundsVolume, Rate }
    private SortType currentSortType = SortType.None;
    private bool isAscending = true;

    private class MeshInfo
    {
        public GameObject NodeObject;
        public string GameObjectName;
        public string MeshName;
        public int VertexCount;
        public float BoundsVolume;
        public float Rate; // 頂点数 / 体積
    }

    [MenuItem("MS_Tools/Models/Mesh Vertex Checker")]
    public static void ShowWindow()
    {
        MeshVertexCheckerWindow window = GetWindow<MeshVertexCheckerWindow>("Mesh Checker");
        window.minSize = new Vector2(620, 350);
        window.Show();
    }

    private void AnalyzeObjects()
    {
        meshInfos.Clear();
        totalVertices = 0;

        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("調査対象のオブジェクトが選択されていません。");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            // MeshFilterの抽出
            foreach (MeshFilter mf in obj.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh != null)
                {
                    Renderer renderer = mf.GetComponent<Renderer>();
                    AddMeshInfo(mf.gameObject, mf.sharedMesh, renderer);
                }
            }

            // SkinnedMeshRendererの抽出
            foreach (SkinnedMeshRenderer smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh != null)
                {
                    AddMeshInfo(smr.gameObject, smr.sharedMesh, smr);
                }
            }
        }

        ApplySorting();
    }

    private void AddMeshInfo(GameObject node, Mesh mesh, Renderer renderer)
    {
        Vector3 size = Vector3.zero;

        // RendererのBounds（ワールド空間での実際のサイズ）を取得
        if (renderer != null)
        {
            size = renderer.bounds.size;

            if (size == Vector3.zero)
            {
                size = Vector3.Scale(mesh.bounds.size, node.transform.lossyScale);
                size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            }
        }
        else
        {
            size = Vector3.Scale(mesh.bounds.size, node.transform.lossyScale);
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        }

        float volume = size.x * size.y * size.z;
        int vertexCount = mesh.vertexCount;

        // 【修正箇所】頂点数 / 体積 （体積が0の場合はエラー回避のため0とする）
        float rate = volume > 0f ? (float)vertexCount / volume : 0f;

        meshInfos.Add(new MeshInfo
        {
            NodeObject = node,
            GameObjectName = node.name,
            MeshName = mesh.name,
            VertexCount = vertexCount,
            BoundsVolume = volume,
            Rate = rate
        });
        totalVertices += vertexCount;
    }

    // --- ソート関連の処理 ---

    private void SortMeshInfos(SortType sortType)
    {
        if (currentSortType == sortType)
        {
            isAscending = !isAscending;
        }
        else
        {
            currentSortType = sortType;
            isAscending = true;
        }
        ApplySorting();
    }

    private void ApplySorting()
    {
        if (meshInfos.Count == 0 || currentSortType == SortType.None) return;

        int modifier = isAscending ? 1 : -1;

        switch (currentSortType)
        {
            case SortType.GameObjectName:
                meshInfos.Sort((a, b) => string.Compare(a.GameObjectName, b.GameObjectName) * modifier);
                break;
            case SortType.MeshName:
                meshInfos.Sort((a, b) => string.Compare(a.MeshName, b.MeshName) * modifier);
                break;
            case SortType.VertexCount:
                meshInfos.Sort((a, b) => a.VertexCount.CompareTo(b.VertexCount) * modifier);
                break;
            case SortType.BoundsVolume:
                meshInfos.Sort((a, b) => a.BoundsVolume.CompareTo(b.BoundsVolume) * modifier);
                break;
            case SortType.Rate:
                meshInfos.Sort((a, b) => a.Rate.CompareTo(b.Rate) * modifier);
                break;
        }
    }

    private string GetHeaderLabel(string baseName, SortType sortType)
    {
        if (currentSortType == sortType) return baseName + (isAscending ? " ▲" : " ▼");
        return baseName;
    }

    // --- GUI描画処理 ---

    private void OnGUI()
    {
        GUILayout.Space(10);

        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("選択オブジェクトからリストを作成 / 更新", GUILayout.Height(30)))
        {
            AnalyzeObjects();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("Project または Hierarchy ビューでオブジェクトを選択し、上のボタンを押してください。", MessageType.Info);
        GUILayout.Space(10);

        if (meshInfos.Count == 0) return;

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label($"合計頂点数: {totalVertices:N0}", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        DrawHeaders();
        DrawList();
    }

    private void DrawHeaders()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button(GetHeaderLabel("GameObject Node", SortType.GameObjectName), EditorStyles.toolbarButton, GUILayout.Width(150))) SortMeshInfos(SortType.GameObjectName);
        if (GUILayout.Button(GetHeaderLabel("Mesh Asset Name", SortType.MeshName), EditorStyles.toolbarButton, GUILayout.MinWidth(100))) SortMeshInfos(SortType.MeshName);
        if (GUILayout.Button(GetHeaderLabel("Vertices", SortType.VertexCount), EditorStyles.toolbarButton, GUILayout.Width(80))) SortMeshInfos(SortType.VertexCount);
        if (GUILayout.Button(GetHeaderLabel("Bounds Volume", SortType.BoundsVolume), EditorStyles.toolbarButton, GUILayout.Width(120))) SortMeshInfos(SortType.BoundsVolume);

        // 【修正箇所】ヘッダーの表記を (Vert/Vol) に変更
        if (GUILayout.Button(GetHeaderLabel("Rate (Vert/Vol)", SortType.Rate), EditorStyles.toolbarButton, GUILayout.Width(100))) SortMeshInfos(SortType.Rate);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawList()
    {
        if (linkStyle == null)
        {
            linkStyle = new GUIStyle(EditorStyles.label);
            linkStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.7f, 1.0f) : new Color(0.1f, 0.3f, 0.8f);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < meshInfos.Count; i++)
        {
            DrawRow(i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawRow(int index)
    {
        MeshInfo info = meshInfos[index];
        Rect rect = EditorGUILayout.BeginHorizontal();

        if (index % 2 == 0)
        {
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.1f));
        }

        if (GUILayout.Button(info.GameObjectName, linkStyle, GUILayout.Width(150)))
        {
            Selection.activeObject = info.NodeObject;
            EditorGUIUtility.PingObject(info.NodeObject);
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        GUILayout.Label(info.MeshName, GUILayout.MinWidth(100));
        GUILayout.Label(info.VertexCount.ToString("N0"), GUILayout.Width(80));
        GUILayout.Label(info.BoundsVolume.ToString("F3"), GUILayout.Width(120));

        // Rateの表示（変更なし、F4で出力）
        GUILayout.Label(info.Rate.ToString("F4"), GUILayout.Width(100));

        EditorGUILayout.EndHorizontal();
    }
}