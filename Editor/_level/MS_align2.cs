using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FitObjectsByAxisAndLOD : EditorWindow
{
    private enum LODTargetMode { LOD0, LOD1, LOD2, LOD3, Highest, Lowest }
    private enum LocalAxis { X, Y, Z }

    private LODTargetMode lodTarget = LODTargetMode.Highest;
    private LocalAxis localAxis = LocalAxis.X;
    private Transform referenceTransform = null;

    [MenuItem("MS_Tools/Level/整列：選択オブジェクトをローカル軸スケールとLOD指定で詰める")]
    public static void ShowWindow()
    {
        GetWindow<FitObjectsByAxisAndLOD>("ローカル軸+LOD整列ツール");
    }

    void OnGUI()
    {
        GUILayout.Label("最初の選択オブジェクトのローカル軸に沿って整列", EditorStyles.boldLabel);
        localAxis = (LocalAxis)EditorGUILayout.EnumPopup("整列ローカル軸", localAxis);
        lodTarget = (LODTargetMode)EditorGUILayout.EnumPopup("使用するLODレベル", lodTarget);

        if (GUILayout.Button("実行"))
        {
            FitAndAlign();
        }
    }

    void FitAndAlign()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length < 2)
        {
            Debug.LogWarning("2つ以上のオブジェクトを選択してください。");
            return;
        }

        referenceTransform = selected[0].transform;
        Vector3 alignAxis = localAxis switch
        {
            LocalAxis.X => referenceTransform.right,
            LocalAxis.Y => referenceTransform.up,
            LocalAxis.Z => referenceTransform.forward,
            _ => referenceTransform.right
        };

        List<(GameObject obj, Bounds bounds)> entries = new();
        foreach (var obj in selected)
        {
            Bounds b = GetTargetBounds(obj);
            if (b.size != Vector3.zero)
                entries.Add((obj, b));
        }

        if (entries.Count < 2)
        {
            Debug.LogWarning("Rendererが有効なオブジェクトが2つ以上必要です。");
            return;
        }

        Bounds totalBounds = entries[0].bounds;
        for (int i = 1; i < entries.Count; i++)
            totalBounds.Encapsulate(entries[i].bounds);

        float totalOriginalSize = 0f;
        foreach (var e in entries)
            totalOriginalSize += Vector3.Project(e.bounds.size, alignAxis).magnitude;

        float targetSize = Vector3.Project(totalBounds.size, alignAxis).magnitude;
        float scaleRatio = targetSize / totalOriginalSize;

        // スケーリング（指定ローカル軸方向のみ）
        foreach (var e in entries)
        {
            GameObject obj = e.obj;
            Undo.RecordObject(obj.transform, "Scale");
            Vector3 scale = obj.transform.localScale;
            Vector3 axisInLocal = referenceTransform.InverseTransformDirection(alignAxis).normalized;
            obj.transform.localScale += Vector3.Scale(axisInLocal, scale * (scaleRatio - 1f));
        }

        // 更新後のBounds取得
        List<(GameObject obj, Bounds bounds)> updated = new();
        foreach (var e in entries)
            updated.Add((e.obj, GetTargetBounds(e.obj)));

        updated.Sort((a, b) => Vector3.Dot(a.bounds.min, alignAxis).CompareTo(Vector3.Dot(b.bounds.min, alignAxis)));

        Vector3 cursor = totalBounds.min;
        cursor = Vector3.Project(cursor - referenceTransform.position, alignAxis) + referenceTransform.position;

        foreach (var (obj, bounds) in updated)
        {
            float size = Vector3.Project(bounds.size, alignAxis).magnitude;
            Vector3 min = Vector3.Project(bounds.min - referenceTransform.position, alignAxis) + referenceTransform.position;
            Vector3 offset = cursor - min;

            Undo.RecordObject(obj.transform, "Move");
            obj.transform.position += offset;

            cursor += alignAxis.normalized * size;
        }
    }

    Bounds GetTargetBounds(GameObject obj)
    {
        var lodGroup = obj.GetComponentInChildren<LODGroup>();
        if (lodGroup != null)
        {
            var lods = lodGroup.GetLODs();
            int index = lodTarget switch
            {
                LODTargetMode.Highest => 0,
                LODTargetMode.Lowest => lods.Length - 1,
                _ => (int)lodTarget
            };

            if (index >= 0 && index < lods.Length)
            {
                Bounds? b = null;
                foreach (var r in lods[index].renderers)
                {
                    if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                    if (b == null) b = r.bounds;
                    else b = Encapsulate(b.Value, r.bounds);
                }
                if (b != null) return b.Value;
            }
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
        Bounds? bounds = null;
        foreach (Renderer r in renderers)
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
            if (bounds == null) bounds = r.bounds;
            else bounds = Encapsulate(bounds.Value, r.bounds);
        }
        return bounds ?? new Bounds(obj.transform.position, Vector3.zero);
    }

    Bounds Encapsulate(Bounds a, Bounds b)
    {
        a.Encapsulate(b.min);
        a.Encapsulate(b.max);
        return a;
    }
}