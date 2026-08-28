using UnityEditor;
using UnityEngine;
using System.Linq;

public class LODPrefabBatchDistanceSetter : EditorWindow
{
    private Camera referenceCamera;

    private float startDistance = 10f; // LOD1が開始する距離（LOD0の終了）
    private float endDistance = 50f;   // カリングされる距離（最後のLODの終了）

    [MenuItem("MS_Tools/Model/LOD 距離で一括設定 (Prefabバッチ処理)")]
    static void Init()
    {
        LODPrefabBatchDistanceSetter window = (LODPrefabBatchDistanceSetter)EditorWindow.GetWindow(typeof(LODPrefabBatchDistanceSetter));
        window.titleContent = new GUIContent("LOD Batch Setter");
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

        EditorGUILayout.LabelField("Prefab LOD Transition Settings", EditorStyles.boldLabel);
        startDistance = EditorGUILayout.FloatField("LOD開始距離 (LOD0終了)", startDistance);
        endDistance = EditorGUILayout.FloatField("カリング距離 (LOD終了)", endDistance);

        if (startDistance >= endDistance)
        {
            EditorGUILayout.HelpBox("カリング距離はLOD開始距離より大きい値にしてください。", MessageType.Error);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("選択中のプレファブアセットに適用"))
        {
            ApplyToSelectedPrefabs();
        }
    }

    void ApplyToSelectedPrefabs()
    {
        // プロジェクトウィンドウで選択されているアセットの中から、プレファブのみを抽出
        GameObject[] selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets)
            .Where(go => PrefabUtility.IsPartOfPrefabAsset(go)).ToArray();

        if (selectedPrefabs.Length == 0)
        {
            Debug.LogWarning("プレファブアセットが選択されていません。プロジェクトウィンドウでプレファブを選択してください。");
            return;
        }

        int processedPrefabCount = 0;
        int processedLODGroupCount = 0;

        foreach (GameObject prefabAsset in selectedPrefabs)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefabAsset);

            // プレファブの内容をメモリ上に展開して編集可能にする
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // 子オブジェクトも含めてすべてのLODGroupを取得
            LODGroup[] lodGroups = contentsRoot.GetComponentsInChildren<LODGroup>(true);

            if (lodGroups.Length > 0)
            {
                foreach (LODGroup lodGroup in lodGroups)
                {
                    ApplyLODTransitionByDistance(lodGroup);
                    processedLODGroupCount++;
                }

                // 変更をプレファブアセットに保存
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
                processedPrefabCount++;
            }

            // メモリ上に展開したプレファブを破棄（必須）
            PrefabUtility.UnloadPrefabContents(contentsRoot);
        }

        if (processedPrefabCount > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"バッチ処理完了: {processedPrefabCount}個のプレファブに含まれる、合計{processedLODGroupCount}個のLODGroupを更新しました。");
        }
        else
        {
            Debug.Log("選択されたプレファブの中にLODGroupを持つものはありませんでした。");
        }
    }

    void ApplyLODTransitionByDistance(LODGroup lodGroup)
    {
        Bounds bounds = GetObjectBounds(lodGroup);
        LOD[] lods = lodGroup.GetLODs();
        int lodCount = lods.Length;

        if (lodCount == 0) return;

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
        }

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
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