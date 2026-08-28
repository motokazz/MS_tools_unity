using UnityEngine;
using UnityEditor;

public class SmartObjectAlignerLODSelectable : EditorWindow
{
    enum AlignDirection { X, Y, Z }
    enum LODReference { Highest, Lowest }

    private AlignDirection direction = AlignDirection.X;
    private float margin = 0.1f;
    private LODReference lodReference = LODReference.Highest;

    [MenuItem("MS_Tools/Level/整列：スマート（LOD選択付き）")]
    public static void ShowWindow()
    {
        GetWindow<SmartObjectAlignerLODSelectable>("スマート整列ツール");
    }

    void OnGUI()
    {
        GUILayout.Label("選択オブジェクトを重ならないよう整列", EditorStyles.boldLabel);
        direction = (AlignDirection)EditorGUILayout.EnumPopup("整列方向", direction);
        margin = EditorGUILayout.FloatField("間隔（マージン）", margin);
        lodReference = (LODReference)EditorGUILayout.EnumPopup("LOD参照レベル", lodReference);

        if (GUILayout.Button("整列実行"))
        {
            AlignObjects();
        }
    }

    void AlignObjects()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length < 2)
        {
            Debug.LogWarning("2つ以上のオブジェクトを選択してください。");
            return;
        }

        // 並び順をソート
        System.Array.Sort(selected, (a, b) =>
        {
            float aPos = GetAxisValue(GetVisibleBounds(a).center);
            float bPos = GetAxisValue(GetVisibleBounds(b).center);
            return aPos.CompareTo(bPos);
        });

        Vector3 current = selected[0].transform.position;

        for (int i = 0; i < selected.Length; i++)
        {
            GameObject obj = selected[i];
            Bounds bounds = GetVisibleBounds(obj);

            if (i > 0)
            {
                Bounds prev = GetVisibleBounds(selected[i - 1]);
                float prevSize = GetBoundsSize(prev);
                float currentSize = GetBoundsSize(bounds);
                current = AddToAxis(current, (prevSize / 2f) + (currentSize / 2f) + margin);
            }

            Vector3 newPos = SetAxisValue(obj.transform.position, GetAxisValue(current));
            Undo.RecordObject(obj.transform, "Align Objects");
            obj.transform.position = newPos;
        }
    }

    Bounds GetVisibleBounds(GameObject obj)
    {
        // LODGroup処理
        LODGroup lodGroup = obj.GetComponentInChildren<LODGroup>();
        if (lodGroup != null)
        {
            var lods = lodGroup.GetLODs();
            if (lods.Length > 0)
            {
                int index = lodReference == LODReference.Highest ? 0 : lods.Length - 1;
                Renderer[] targetRenderers = lods[index].renderers;

                Bounds? lodBounds = null;
                foreach (var r in targetRenderers)
                {
                    if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;

                    if (lodBounds == null)
                        lodBounds = r.bounds;
                    else
                        lodBounds.Value.Encapsulate(r.bounds);
                }

                if (lodBounds != null) return lodBounds.Value;
            }
        }

        // 通常Renderer処理
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
        Bounds? combined = null;

        foreach (Renderer r in renderers)
        {
            if (!r.enabled || !r.gameObject.activeInHierarchy) continue;

            if (combined == null)
                combined = r.bounds;
            else
                combined.Value.Encapsulate(r.bounds);
        }

        return combined ?? new Bounds(obj.transform.position, Vector3.zero);
    }

    float GetAxisValue(Vector3 v)
    {
        return direction switch
        {
            AlignDirection.X => v.x,
            AlignDirection.Y => v.y,
            AlignDirection.Z => v.z,
            _ => v.x
        };
    }

    Vector3 SetAxisValue(Vector3 v, float value)
    {
        return direction switch
        {
            AlignDirection.X => new Vector3(value, v.y, v.z),
            AlignDirection.Y => new Vector3(v.x, value, v.z),
            AlignDirection.Z => new Vector3(v.x, v.y, value),
            _ => v
        };
    }

    Vector3 AddToAxis(Vector3 v, float delta)
    {
        return direction switch
        {
            AlignDirection.X => v + new Vector3(delta, 0, 0),
            AlignDirection.Y => v + new Vector3(0, delta, 0),
            AlignDirection.Z => v + new Vector3(0, 0, delta),
            _ => v
        };
    }

    float GetBoundsSize(Bounds b)
    {
        return direction switch
        {
            AlignDirection.X => b.size.x,
            AlignDirection.Y => b.size.y,
            AlignDirection.Z => b.size.z,
            _ => b.size.x
        };
    }
}
