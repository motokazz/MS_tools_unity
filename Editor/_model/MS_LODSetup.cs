using UnityEngine;
using UnityEditor;

public class MoveMeshAndSetupTwoLODs : EditorWindow
{
    [MenuItem("MS_Tools/Model/Move Mesh To Child and Setup 2 LODs")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MoveMeshAndSetupTwoLODs));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("MS_LODSetup");
        GUILayout.BeginVertical();


        // SelectSourcePrefabs
        if (GUILayout.Button("MoveAndSetupLOD1", GUILayout.Width(200), GUILayout.Height(30)))
        {
            MoveAndSetupLOD1();
        }

        // SelectSourcePrefabs
        if (GUILayout.Button("MoveAndSetupLOD2", GUILayout.Width(200), GUILayout.Height(30)))
        {
            MoveAndSetupLOD2();
        }


        // LODGroupSetup
        if (GUILayout.Button("LODGroupSetup", GUILayout.Width(200), GUILayout.Height(30)))
        {
            LODGroupSetup();
        }

        // LODGroupSetup2
        if (GUILayout.Button("LODGroupSetup2", GUILayout.Width(200), GUILayout.Height(30)))
        {
            LODGroupSetup2();
        }

        GUILayout.EndVertical();
    }

    static void MoveAndSetupLOD1()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("オブジェクトが選択されていません。");
            return;
        }

        MeshFilter mf = selected.GetComponent<MeshFilter>();
        MeshRenderer mr = selected.GetComponent<MeshRenderer>();

        if (mf == null || mr == null)
        {
            Debug.LogWarning("MeshFilter または MeshRenderer が見つかりません。");
            return;
        }

        // LOD0オブジェクト作成
        GameObject lod0Obj = new GameObject("LOD0_Mesh");
        lod0Obj.transform.SetParent(selected.transform);
        lod0Obj.transform.localPosition = Vector3.zero;
        lod0Obj.transform.localRotation = Quaternion.identity;
        lod0Obj.transform.localScale = Vector3.one;

        MeshFilter lod0MF = lod0Obj.AddComponent<MeshFilter>();
        MeshRenderer lod0MR = lod0Obj.AddComponent<MeshRenderer>();
        lod0MF.sharedMesh = mf.sharedMesh;
        lod0MR.sharedMaterials = mr.sharedMaterials;


        // 元のMeshFilter / Rendererを削除
        DestroyImmediate(mf);
        DestroyImmediate(mr);

        // LODGroup セットアップ
        LODGroup lodGroup = selected.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = selected.AddComponent<LODGroup>();
        }

        LOD[] lods = new LOD[1];
        lods[0] = new LOD(0.0001f, new Renderer[] { lod0MR });  // LOD0: 表示距離 2%

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        Debug.Log("LOD0およびLOD1をセットアップしました。");
    }

    static void MoveAndSetupLOD2()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("オブジェクトが選択されていません。");
            return;
        }

        MeshFilter mf = selected.GetComponent<MeshFilter>();
        MeshRenderer mr = selected.GetComponent<MeshRenderer>();

        if (mf == null || mr == null)
        {
            Debug.LogWarning("MeshFilter または MeshRenderer が見つかりません。");
            return;
        }

        // LOD0オブジェクト作成
        GameObject lod0Obj = new GameObject("LOD0_Mesh");
        lod0Obj.transform.SetParent(selected.transform);
        lod0Obj.transform.localPosition = Vector3.zero;
        lod0Obj.transform.localRotation = Quaternion.identity;
        lod0Obj.transform.localScale = Vector3.one;

        MeshFilter lod0MF = lod0Obj.AddComponent<MeshFilter>();
        MeshRenderer lod0MR = lod0Obj.AddComponent<MeshRenderer>();
        lod0MF.sharedMesh = mf.sharedMesh;
        lod0MR.sharedMaterials = mr.sharedMaterials;

        // LOD1オブジェクト作成（同じメッシュを使うが、後で差し替え可能）
        GameObject lod1Obj = new GameObject("LOD1_Mesh");
        lod1Obj.transform.SetParent(selected.transform);
        lod1Obj.transform.localPosition = Vector3.zero;
        lod1Obj.transform.localRotation = Quaternion.identity;
        lod1Obj.transform.localScale = Vector3.one;

        MeshFilter lod1MF = lod1Obj.AddComponent<MeshFilter>();
        MeshRenderer lod1MR = lod1Obj.AddComponent<MeshRenderer>();
        lod1MF.sharedMesh = mf.sharedMesh; // ここを簡略化メッシュに差し替え可能
        lod1MR.sharedMaterials = mr.sharedMaterials;

        // 元のMeshFilter / Rendererを削除
        DestroyImmediate(mf);
        DestroyImmediate(mr);

        // LODGroup セットアップ
        LODGroup lodGroup = selected.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = selected.AddComponent<LODGroup>();
        }

        LOD[] lods = new LOD[2];
        lods[0] = new LOD(0.0002f, new Renderer[] { lod0MR });  // LOD0: 表示距離 2%
        lods[1] = new LOD(0.0001f, new Renderer[] { lod1MR });  // LOD1: 表示距離 1%

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        Debug.Log("LOD0およびLOD1をセットアップしました。");
    }

    static void LODGroupSetup()
    {
        GameObject selected = Selection.activeGameObject;
        // LODGroup セットアップ
        LODGroup lodGroup = selected.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = selected.AddComponent<LODGroup>();
        }

        //MeshRendererダミー
        MeshRenderer lod0MR = new MeshRenderer();

        LOD[] lods = new LOD[1];
        lods[0] = new LOD(0.0001f, new Renderer[] { lod0MR });  // LOD0: 表示距離 2%

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        Debug.Log("LOD0およびLOD1をセットアップしました。");
    }
    static void LODGroupSetup2()
    {
        GameObject selected = Selection.activeGameObject;
        // LODGroup セットアップ
        LODGroup lodGroup = selected.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = selected.AddComponent<LODGroup>();
        }
        //MeshRendererダミー
        MeshRenderer lod0MR = new MeshRenderer();

        LOD[] lods = new LOD[2];
        lods[0] = new LOD(0.0002f, new Renderer[] { lod0MR });  // LOD0: 表示距離 2%
        lods[1] = new LOD(0.0001f, new Renderer[] { lod0MR });  // LOD1: 表示距離 1%

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        Debug.Log("LOD0およびLOD1をセットアップしました。");
    }
}
