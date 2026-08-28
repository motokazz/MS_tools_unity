using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ObjectFitterTool : EditorWindow
{
    GameObject container;
    GameObject target;
    List<GameObject> targets = new List<GameObject>();

    [MenuItem("Tools/Fit Objects Tool")]
    static void Init() => GetWindow<ObjectFitterTool>("Fit Into Box");

    void OnGUI()
    {
        container = (GameObject)EditorGUILayout.ObjectField("Container", container, typeof(GameObject), true);

        target = (GameObject)EditorGUILayout.ObjectField("target", target, typeof(GameObject), true);
        if (GUILayout.Button("Collect Selected Objects"))
        {
            targets.Clear();
            targets.AddRange(Selection.gameObjects);
        }

        if (GUILayout.Button("Arrange Objects"))
        {
                FitPerfect();
        }
    }

    // ===== 完全フィット詰め =====
    Bounds GetLocalBoundsRelativeTo(Transform reference, GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        // reference基準のローカル空間で計算
        Matrix4x4 worldToLocal = reference.worldToLocalMatrix;

        Bounds localBounds = new Bounds(
            worldToLocal.MultiplyPoint3x4(renderers[0].bounds.center),
            Vector3.zero
        );

        foreach (var r in renderers)
        {
            Bounds b = r.bounds; // ワールド空間のAABB
            Vector3 center = worldToLocal.MultiplyPoint3x4(b.center);

            // sizeはベクトル変換で軸ごとに求める（スケールの影響だけ反映）
            Vector3 size = new Vector3(
                Vector3.Dot(worldToLocal.MultiplyVector(new Vector3(b.size.x, 0, 0)), Vector3.right),
                Vector3.Dot(worldToLocal.MultiplyVector(new Vector3(0, b.size.y, 0)), Vector3.up),
                Vector3.Dot(worldToLocal.MultiplyVector(new Vector3(0, 0, b.size.z)), Vector3.forward)
            );

            Bounds lb = new Bounds(center, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z)));
            localBounds.Encapsulate(lb);
        }

        return localBounds;
    }

    void FitPerfect()
    {
        if (container == null || targets.Count == 0) return;

        //
        //コンテナ

        Transform containerParent = container.transform.parent;
        container.transform.parent = null;
        var containerTransformOriginPosition = container.transform.position;
        var containerTransformOriginRotation = container.transform.rotation;
        container.transform.position = Vector3.zero;
        container.transform.rotation = Quaternion.identity;

        //コンテナのサイズ取得
        Bounds boxBoundsLocal = container.GetComponent<Renderer>().bounds;
        Vector3 boxSize = VectorFix(boxBoundsLocal.size);
        Debug.Log(boxSize);

        //コンテナ元に戻す
        container.transform.position = containerTransformOriginPosition;
        container.transform.rotation = containerTransformOriginRotation;
        container.transform.parent = containerParent;

        //
        //ターゲットを詰め込む親

        Transform contained = new GameObject("contained").transform;


        //
        //ターゲット

        var targetOBJ = GameObject.Instantiate(target);
        Bounds targetBoundsLocal = targetOBJ.GetComponentInChildren<Renderer>().localBounds;
        Vector3 targetSize = VectorFix( targetBoundsLocal.size);
        Debug.Log(targetSize);
        //ターゲットセンターギャップ記録
        Vector3 centerGap = targetOBJ.transform.position - targetBoundsLocal.center;

        DestroyImmediate(targetOBJ);


        //何個詰め込めるか判定
        int xcount = (int)Mathf.Round(boxSize.x / targetSize.x);
        int ycount = (int)Mathf.Round(boxSize.y / targetSize.y);
        int zcount = (int)Mathf.Round(boxSize.z / targetSize.z);
        int count = xcount * zcount;

        //箱に詰め込むサイズ

        if (xcount <= 0) xcount = 1;
        if (ycount <= 0) ycount = 1;
        if (zcount <= 0) zcount = 1;

        Vector3 cellSize = new Vector3(
            boxSize.x / xcount,
            boxSize.y / ycount,
            boxSize.z / zcount
        );
        Debug.Log(cellSize);

        //
        //詰め込み
        //
        //詰め込み始めるポジション
        Vector3 startPos = boxBoundsLocal.min;

        for (int i = 0; i < count; i++)
        {
            for(int j  = 0; j < ycount; j++)
            {
                GameObject go = GameObject.Instantiate(target);

                //スケーリング

                go.transform.localScale = new Vector3(cellSize.x/targetSize.x, cellSize.y / targetSize.y, cellSize.z / targetSize.z);

                // セルの中心
                int row = i / xcount;
                int col = i % xcount;
                int height = j + 1;

                Vector3 cellCenterLocal = startPos +
                    new Vector3(cellSize.x * col + cellSize.x / 2f,
                                cellSize.y * j + cellSize.y / 2f,
                                cellSize.z * row + cellSize.z / 2f
                                );

                // 中心補正
                Vector3 offset = cellCenterLocal - Vector3.Scale(targetBoundsLocal.center, go.transform.localScale);

                // ローカル→ワールド変換
                go.transform.position = offset;

                // 親子付け
                go.transform.parent = contained;
            }
        }

        //
        //詰め込んだ後移動
        //
        contained.position = containerTransformOriginPosition;
        contained.rotation = containerTransformOriginRotation;
    }

    Vector3 VectorFix(Vector3 vec)
    {
        if (vec.x <= 0) vec.x = 1;
        if (vec.y <= 0) vec.y = 1;
        if (vec.z <= 0) vec.z = 1;
        return vec;
    }
}
