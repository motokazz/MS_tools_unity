// Assets/Editor/ColliderToMeshGenerator.cs
using UnityEngine;
using UnityEditor;

public class ColliderToMeshGenerator : EditorWindow
{
    [MenuItem("MS_Tools/Collider/Collider → Mesh Generator")]
    public static void ShowWindow()
    {
        GetWindow<ColliderToMeshGenerator>("Collider → Mesh");
    }

    void OnGUI()
    {
        GUILayout.Label("選択中オブジェクトのColliderをMesh化", EditorStyles.boldLabel);

        if (GUILayout.Button("生成"))
        {
            foreach (var obj in Selection.gameObjects)
            {
                GenerateFromCollider(obj);
            }
        }
    }

    static void GenerateFromCollider(GameObject source)
    {
        if (source == null) return;

        var meshCol = source.GetComponent<MeshCollider>();
        if (meshCol != null && meshCol.sharedMesh != null)
        {
            CreateMeshObject(meshCol.sharedMesh, source.transform, "MeshColliderMesh");
        }

        var boxCol = source.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Mesh cubeMesh = CreateCubeMesh();
            GameObject go = CreateMeshObject(cubeMesh, source.transform, "BoxColliderMesh");

            // center と size を反映
            go.transform.localPosition = boxCol.center;
            go.transform.localScale = boxCol.size;
        }
    }

    static GameObject CreateMeshObject(Mesh mesh, Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        mf.sharedMesh = mesh;

        // ✅ URP Lit マテリアルを作成
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("URP Lit Shader が見つかりません。URPプロジェクトか確認してください。");
            return go;
        }

        var mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(1f, 0f, 0f, 0.4f)); // 半透明の赤

        // 透明にするためのブレンド設定
        mat.SetFloat("_Surface", 1); // 0=Opaque, 1=Transparent
        mat.SetFloat("_Blend", 0); // Alpha
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        mr.sharedMaterial = mat;

        return go;
    }

    static Mesh CreateCubeMesh()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.DestroyImmediate(temp);
        return mesh;
    }
}
