using UnityEditor;
using UnityEngine;
using System.IO;

public class MeshMergerEditor : EditorWindow
{
    [MenuItem("MS_Tools/Model/Mesh Merger")]
    public static void ShowWindow()
    {
        GetWindow<MeshMergerEditor>("Mesh Merger");
    }

    void OnGUI()
    {
        GUILayout.Label("Mesh Merger Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Merge and Save Selected Meshes"))
        {
            MergeAndSaveSelectedMeshes();
        }
    }

    void MergeAndSaveSelectedMeshes()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length < 2)
        {
            EditorUtility.DisplayDialog("Error", "2つ以上のGameObjectを選択してください。", "OK");
            return;
        }

        // メッシュフィルターの取得
        var meshFilters = new System.Collections.Generic.List<MeshFilter>();
        foreach (var obj in selectedObjects)
        {
            var mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                meshFilters.Add(mf);
            }

            //子孫
            var childlensMF = obj.transform.GetComponentsInChildren<MeshFilter>();
            foreach (var item in childlensMF)
            {
                meshFilters.Add(item);
            }
        }

        if (meshFilters.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "選択したオブジェクトにメッシュが見つかりません。", "OK");
            return;
        }

        // Combine meshes
        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "MergedMesh";
        combinedMesh.CombineMeshes(combine);

        // Save mesh as asset
        string folderPath = "Assets/MergedMeshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "MergedMeshes");
        }

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/MergedMesh.asset");
        AssetDatabase.CreateAsset(combinedMesh, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Create new GameObject
        GameObject mergedObject = new GameObject("MergedObject");
        mergedObject.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        var renderer = mergedObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = meshFilters[0].GetComponent<Renderer>().sharedMaterial;

        Selection.activeGameObject = mergedObject;

        EditorUtility.DisplayDialog("成功", $"マージしたメッシュを保存しました：\n{uniquePath}", "OK");
    }
}
