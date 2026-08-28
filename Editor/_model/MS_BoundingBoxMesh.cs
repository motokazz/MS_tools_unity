using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BoundingBoxMesh : EditorWindow
{
    public bool enableMaterialCopy = false;




    [MenuItem("MS_Tools/Model/CreateBB")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(BoundingBoxMesh));

    }

    private void OnGUI()
    {
        // EditorGUILayoutの使用例.
        EditorGUILayout.LabelField("BoundingBoxMesh");
        GUILayout.BeginVertical();


        // SelectSourcePrefabs
        if (GUILayout.Button("BoundingBoxMesh", GUILayout.Width(200), GUILayout.Height(30)))
        {
            CreateBB();
        }
        //
        enableMaterialCopy = EditorGUILayout.Toggle("enableMaterialCopy", enableMaterialCopy);

        GUILayout.EndVertical();
    }

    void CreateBB()
    {
        GameObject selected = Selection.activeGameObject;

        // 元オブジェクトのバウンディングボックス取得
        Renderer rend = selected.GetComponent<Renderer>();
        Bounds bounds = rend.bounds;

        // メッシュ作成用のGameObject作成
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = selected.name + "_BoundingBoxMesh";


        // バウンディングボックスと同じ位置・サイズに調整
        box.transform.position = bounds.center;
        box.transform.localScale = bounds.size;

        // オブジェクトの子にする（任意）
        box.transform.SetParent(selected.transform.parent); 


        // マテリアルを元オブジェクトからコピー
        if (enableMaterialCopy)
        {
            Material originalMat = rend.sharedMaterial;
            box.GetComponent<Renderer>().material = originalMat;
        }
        else
        {
            // 任意でワイヤーフレーム表示にするにはマテリアル変更
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            box.GetComponent<Renderer>().material = mat;
        }

        // 任意：バウンディングボックスのColliderは不要なら削除
        Destroy(box.GetComponent<Collider>());


    }
}
