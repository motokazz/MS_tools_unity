using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LODDistanceSetter : EditorWindow
{
    private Camera referenceCamera;

    // 設定する2つの距離
    private float startDistance = 10f; // LOD1が開始する距離（LOD0の終了）
    private float endDistance = 50f;   // カリングされる距離（最後のLODの終了）

    [MenuItem("MS_Tools/Model/LOD 距離で一括設定")]
    static void Init()
    {
        LODDistanceSetter window = (LODDistanceSetter)EditorWindow.GetWindow(typeof(LODDistanceSetter));
        window.titleContent = new GUIContent("LOD Distance Setter");
        window.Show();
    }

    void OnGUI()
    {
        referenceCamera = SceneView.lastActiveSceneView?.camera;

        if (referenceCamera == null)
        {
            EditorGUILayout.HelpBox("シーンビューのカメラが見つかりません。", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("LOD Transition Settings", EditorStyles.boldLabel);
        startDistance = EditorGUILayout.FloatField("LOD開始距離 (LOD0終了)", startDistance);
        endDistance = EditorGUILayout.FloatField("カリング距離 (LOD終了)", endDistance);

        if (startDistance >= endDistance)
        {
            EditorGUILayout.HelpBox("カリング距離はLOD開始距離より大きい値にしてください。", MessageType.Error);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("選択中のLODGroupに適用"))
        {
            ApplyToSelected();
        }
    }

    void ApplyToSelected()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("GameObjectが選択されていません。");
            return;
        }

        // 複数オブジェクトを選択して実行した際、1回のUndo操作で全員まとめて元に戻せるようにグループ化
        Undo.IncrementCurrentGroup();
        int undoGroupIndex = Undo.GetCurrentGroup();

        int count = 0;
        foreach (GameObject go in selected)
        {
            LODGroup lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) continue;

            ApplyLODTransitionByDistance(lodGroup);
            count++;
        }

        Undo.CollapseUndoOperations(undoGroupIndex);
        Debug.Log($"LOD設定を {count} 個のLODGroupに適用しました。");
    }

    void ApplyLODTransitionByDistance(LODGroup lodGroup)
    {
        Bounds bounds = GetObjectBounds(lodGroup);
        LOD[] lods = lodGroup.GetLODs();
        int lodCount = lods.Length;

        if (lodCount == 0) return;

        // 【重要】変更前にオブジェクトをUndoシステムに登録し、Unityに変更（Dirty）を認識させます
        Undo.RecordObject(lodGroup, "Apply LOD Distances");

        for (int i = 0; i < lodCount; i++)
        {
            float desiredDistance;

            if (lodCount == 1)
            {
                desiredDistance = endDistance;
            }
            else
            {
                desiredDistance = Mathf.Lerp(startDistance, endDistance, (float)i / (lodCount - 1));
            }

            float transitionHeight = GetTransitionHeightAsUnityDoes(bounds, referenceCamera, desiredDistance);

            if (i > 0 && transitionHeight >= lods[i - 1].screenRelativeTransitionHeight)
            {
                transitionHeight = lods[i - 1].screenRelativeTransitionHeight - 0.01f;
            }

            lods[i].screenRelativeTransitionHeight = Mathf.Clamp01(transitionHeight);
            Debug.Log($"[{lodGroup.gameObject.name}] LOD{i} Distance: {desiredDistance:F1}m => TransitionHeight: {transitionHeight:F4}");
        }

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        // 【重要】プレファブインスタンスのプロパティ変更を明示的に記録し、インスペクター上で太字にします
        PrefabUtility.RecordPrefabInstancePropertyModifications(lodGroup);
    }

    Bounds GetObjectBounds(LODGroup lodGroup)
    {
        Renderer[] renderers = lodGroup.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(lodGroup.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    float GetTransitionHeightAsUnityDoes(Bounds bounds, Camera cam, float distance)
    {
        if (cam == null || bounds.size.magnitude == 0)
            return 1f;

        Vector3 worldSpaceSize = bounds.size;
        float objectSize = Mathf.Max(worldSpaceSize.x, worldSpaceSize.y, worldSpaceSize.z);

        if (cam.orthographic)
        {
            float vertical = cam.orthographicSize * 2f;
            return objectSize / vertical;
        }
        else
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return objectSize / frustumHeight;
        }
    }
}