using UnityEditor;
using UnityEngine;
using System.Linq;

public class LODPrefabBatchDistanceSetter : EditorWindow
{
    private Camera referenceCamera;

    private float startDistance = 10f; // LOD1が開始する距離（LOD0の終了）
    private float endDistance = 50f;   // カリングされる距離（最後のLODの終了）

    [MenuItem("MS_Tools/Models/LOD 距離で一括設定 (バッチ処理)")]
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

        EditorGUILayout.LabelField("LOD Transition Settings", EditorStyles.boldLabel);
        startDistance = EditorGUILayout.FloatField("LOD開始距離 (LOD0終了)", startDistance);
        endDistance = EditorGUILayout.FloatField("カリング距離 (LOD終了)", endDistance);

        if (startDistance >= endDistance)
        {
            EditorGUILayout.HelpBox("カリング距離はLOD開始距離より大きい値にしてください。", MessageType.Error);
        }

        GUILayout.Space(10);

        // Projectビュー（Prefab）用のボタン
        if (GUILayout.Button("Projectの選択プレファブに適用"))
        {
            ApplyToSelectedPrefabs();
        }

        GUILayout.Space(5);

        // Hierarchyビュー（Sceneオブジェクト）用のボタン
        if (GUILayout.Button("Hierarchyの選択オブジェクトに適用"))
        {
            ApplyToSelectedGameObjects();
        }
    }

    void ApplyToSelectedPrefabs()
    {
        GameObject[] selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets)
            .Where(go => PrefabUtility.IsPartOfPrefabAsset(go)).ToArray(); //[cite: 1]

        if (selectedPrefabs.Length == 0)
        {
            Debug.LogWarning("プレファブアセットが選択されていません。");
            return;
        }

        int processedPrefabCount = 0;
        int processedLODGroupCount = 0;

        foreach (GameObject prefabAsset in selectedPrefabs)
        {
            string assetPath = AssetDatabase.GetAssetPath(prefabAsset); //[cite: 1]
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath); //[cite: 1]
            LODGroup[] lodGroups = contentsRoot.GetComponentsInChildren<LODGroup>(true); //[cite: 1]

            if (lodGroups.Length > 0)
            {
                foreach (LODGroup lodGroup in lodGroups)
                {
                    ApplyLODTransitionByDistance(lodGroup); //[cite: 1]
                    processedLODGroupCount++;
                }
                PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath); //[cite: 1]
                processedPrefabCount++;
            }
            PrefabUtility.UnloadPrefabContents(contentsRoot); //[cite: 1]
        }

        if (processedPrefabCount > 0)
        {
            AssetDatabase.SaveAssets(); //[cite: 1]
            Debug.Log($"バッチ処理完了: {processedPrefabCount}個のプレファブに含まれる、合計{processedLODGroupCount}個のLODGroupを更新しました。"); //[cite: 1]
        }
    }

    void ApplyToSelectedGameObjects()
    {
        // Hierarchy上で選択されている（Prefabアセットではない）オブジェクトを抽出
        GameObject[] selectedGOs = Selection.gameObjects
            .Where(go => !PrefabUtility.IsPartOfPrefabAsset(go)).ToArray();

        if (selectedGOs.Length == 0)
        {
            Debug.LogWarning("ヒエラルキー上でオブジェクトが選択されていません。");
            return;
        }

        int processedCount = 0;

        foreach (GameObject go in selectedGOs)
        {
            LODGroup[] lodGroups = go.GetComponentsInChildren<LODGroup>(true);
            foreach (LODGroup lodGroup in lodGroups)
            {
                ApplyLODTransitionByDistance(lodGroup);
                processedCount++;
            }
        }

        if (processedCount > 0)
        {
            Debug.Log($"バッチ処理完了: ヒエラルキー上の{processedCount}個のLODGroupを更新しました。");
        }
    }

    void ApplyLODTransitionByDistance(LODGroup lodGroup)
    {
        // Hierarchyでの変更を取り消せるようにUndoを登録
        Undo.RecordObject(lodGroup, "Update LOD Distances");

        // LODGroup内部のバウンディング情報を最新化し、スケール非依存のベースサイズを取得する
        lodGroup.RecalculateBounds(); //[cite: 1]
        float objectSize = lodGroup.size;

        LOD[] lods = lodGroup.GetLODs(); //[cite: 1]
        int lodCount = lods.Length; //[cite: 1]

        if (lodCount == 0) return; //[cite: 1]

        for (int i = 0; i < lodCount; i++) //[cite: 1]
        {
            float desiredDistance;

            if (lodCount == 1) //[cite: 1]
            {
                desiredDistance = endDistance; //[cite: 1]
            }
            else //[cite: 1]
            {
                desiredDistance = Mathf.Lerp(startDistance, endDistance, (float)i / (lodCount - 1)); //[cite: 1]
            }

            float transitionHeight = GetTransitionHeight(objectSize, referenceCamera, desiredDistance);

            // 前のLODの遷移高さより大きくならないようにクランプ処理[cite: 1]
            if (i > 0 && transitionHeight >= lods[i - 1].screenRelativeTransitionHeight) //[cite: 1]
            {
                transitionHeight = lods[i - 1].screenRelativeTransitionHeight - 0.01f; //[cite: 1]
            }

            lods[i].screenRelativeTransitionHeight = Mathf.Clamp01(transitionHeight); //[cite: 1]
        }

        lodGroup.SetLODs(lods); //[cite: 1]
    }

    // 元のGetObjectBounds関数を削除し、サイズを直接引数に取るよう変更
    float GetTransitionHeight(float objectSize, Camera cam, float distance)
    {
        if (cam == null || objectSize == 0)
            return 1f;

        if (cam.orthographic) //[cite: 1]
        {
            float vertical = cam.orthographicSize * 2f; //[cite: 1]
            return objectSize / vertical; //[cite: 1]
        }
        else //[cite: 1]
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad); //[cite: 1]
            return objectSize / frustumHeight; //[cite: 1]
        }
    }
}