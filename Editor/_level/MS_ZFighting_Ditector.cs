using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LODZFightDetector : EditorWindow
{
    private enum LODTarget { Highest, Lowest }
    private LODTarget targetLOD = LODTarget.Highest;
    private float threshold = 0.01f;

    private struct ObjectPair
    {
        public GameObject a;
        public GameObject b;
    }

    private List<ObjectPair> detectedPairs = new List<ObjectPair>();
    private Vector2 scrollPos;

    [MenuItem("MS_Tools/Level/LOD Z-Fighting Detector")]
    public static void ShowWindow()
    {
        GetWindow<LODZFightDetector>("LOD Z-Fighting Detector");
    }

    void OnGUI()
    {
        GUILayout.Label("LOD Z-Fighting Detector", EditorStyles.boldLabel);
        targetLOD = (LODTarget)EditorGUILayout.EnumPopup("Target LOD Level", targetLOD);
        threshold = EditorGUILayout.FloatField("Center Distance Threshold", threshold);

        if (GUILayout.Button("Detect"))
        {
            DetectZFighting();
        }

        if (detectedPairs.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Detected {detectedPairs.Count} Potential Z-Fighting Pairs", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < detectedPairs.Count; i++)
            {
                var pair = detectedPairs[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(pair.a, typeof(GameObject), true);
                EditorGUILayout.ObjectField(pair.b, typeof(GameObject), true);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.objects = new Object[] { pair.a, pair.b };
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Space(10);
            GUILayout.Label("No Z-Fighting pairs detected or not yet analyzed.", EditorStyles.helpBox);
        }
    }

    void DetectZFighting()
    {
        detectedPairs.Clear();

        var lodGroups = GameObject.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);
        var lodObjects = new List<(GameObject go, Bounds bounds, LODGroup group)>();

        foreach (var group in lodGroups)
        {
            var lods = group.GetLODs();
            if (lods.Length == 0) continue;

            LOD targetLod = (targetLOD == LODTarget.Highest) ? lods[0] : lods[lods.Length - 1];
            foreach (var renderer in targetLod.renderers)
            {
                if (renderer == null) continue;
                lodObjects.Add((renderer.gameObject, renderer.bounds, group));
            }
        }

        // ループ内のこの部分を修正
        for (int i = 0; i < lodObjects.Count; i++)
        {
            for (int j = i + 1; j < lodObjects.Count; j++)
            {
                var a = lodObjects[i];
                var b = lodObjects[j];

                // 追加：同一オブジェクト除外
                if (a.go == b.go)
                    continue;

                // 同じLODGroup内は無視
                if (a.group == b.group)
                    continue;

                if (a.bounds.Intersects(b.bounds))
                {
                    float dist = Vector3.Distance(a.bounds.center, b.bounds.center);
                    if (dist < threshold)
                    {
                        detectedPairs.Add(new ObjectPair { a = a.go, b = b.go });
                    }
                }
            }
        }


        if (detectedPairs.Count == 0)
        {
            Debug.Log("No potential Z-Fighting detected.");
        }
        else
        {
            Debug.Log($"Detected {detectedPairs.Count} potential Z-Fighting pairs.");
        }
    }
}
