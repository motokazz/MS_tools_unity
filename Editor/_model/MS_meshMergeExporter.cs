using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MergeLODsPivotExportSafe : EditorWindow
{
    enum LODSelectMode { Highest, Lowest }
    LODSelectMode lodMode = LODSelectMode.Highest;

    const string LAST_FBX_EXPORT_DIR_KEY = "MergedLODs_LastExportPath";

    [MenuItem("MS_Tools/Model/Merge LODs to FBX (Pivot, Safe)")]
    static void Init()
    {
        GetWindow<MergeLODsPivotExportSafe>("LOD Merge & Export").Show();
    }

    void OnGUI()
    {
        GUILayout.Label("LOD Merge Settings", EditorStyles.boldLabel);
        lodMode = (LODSelectMode)EditorGUILayout.EnumPopup("LOD Level", lodMode);

        if (GUILayout.Button("Merge and Export FBX"))
        {
            MergeAndExportLOD(lodMode);
        }
    }

    static void MergeAndExportLOD(LODSelectMode mode)
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていません。");
            return;
        }

        Transform pivotTransform = selectedObjects[0].transform;
        Vector3 pivotPos = pivotTransform.position;
        Quaternion pivotRot = pivotTransform.rotation;

        List<CombineInstance> combineInstances = new List<CombineInstance>();
        List<Material> materials = new List<Material>();

        foreach (GameObject root in selectedObjects)
        {
            var lodGroups = root.GetComponentsInChildren<LODGroup>(true);
            foreach (var lodGroup in lodGroups)
            {
                LOD[] lods = lodGroup.GetLODs();
                if (lods.Length == 0) continue;

                int targetLODIndex = mode == LODSelectMode.Highest ? 0 : lods.Length - 1;

                if (targetLODIndex >= lods.Length) continue;

                foreach (Renderer renderer in lods[targetLODIndex].renderers)
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;

                    MeshFilter mf = renderer.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;

                    Mesh mesh = mf.sharedMesh;
                    Material[] mats = renderer.sharedMaterials;

                    for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    {
                        CombineInstance ci = new CombineInstance();
                        ci.mesh = mesh;
                        ci.subMeshIndex = sub;
                        ci.transform = mf.transform.localToWorldMatrix;
                        combineInstances.Add(ci);

                        materials.Add(mats[Mathf.Min(sub, mats.Length - 1)]);
                    }
                }
            }
        }

        if (combineInstances.Count == 0)
        {
            Debug.LogWarning("指定されたLODに対応するメッシュが見つかりませんでした。");
            return;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray(), false, true);

        Vector3[] verts = combinedMesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = Quaternion.Inverse(pivotRot) * (verts[i] - pivotPos);
        }
        combinedMesh.vertices = verts;
        combinedMesh.RecalculateBounds();

        GameObject mergedObj = new GameObject("Merged_LOD_" + mode.ToString());
        mergedObj.transform.position = Vector3.zero;
        mergedObj.transform.rotation = Quaternion.identity;

        var mfNew = mergedObj.AddComponent<MeshFilter>();
        var mrNew = mergedObj.AddComponent<MeshRenderer>();
        mfNew.sharedMesh = combinedMesh;
        mrNew.sharedMaterials = materials.ToArray();

        // FBX保存パス選択
        string lastDir = EditorPrefs.GetString(LAST_FBX_EXPORT_DIR_KEY, "Assets");
        string path = EditorUtility.SaveFilePanel("Export FBX", lastDir, mergedObj.name, "fbx");

        if (!string.IsNullOrEmpty(path))
        {
            string exportDir = Path.GetDirectoryName(path);
            EditorPrefs.SetString(LAST_FBX_EXPORT_DIR_KEY, exportDir);

            System.Type exporterType = System.Type.GetType("UnityEditor.Formats.Fbx.Exporter.ModelExporter, Unity.Formats.Fbx.Editor");
            if (exporterType != null)
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                exporterType
                    .GetMethod("ExportObject", new System.Type[] { typeof(string), typeof(GameObject) })
                    .Invoke(null, new object[] { relativePath, mergedObj });

                Debug.Log("✅ FBXエクスポート完了: " + relativePath);
            }
            else
            {
                Debug.LogWarning("⚠ FBX Exporter が見つかりませんでした。出力はスキップされました。");
            }
        }

        DestroyImmediate(mergedObj);
    }
}
