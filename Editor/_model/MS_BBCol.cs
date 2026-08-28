using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using System.Linq;

public class MultiBoundsBoxColliderWindow : EditorWindow
{
    [MenuItem("MS_Tools/Collider/Create Merged Bounds BoxCollider")]
    public static void ShowWindow()
    {
        GetWindow<MultiBoundsBoxColliderWindow>("Multi-Object Bounds");
    }

    private void OnGUI()
    {
        GUILayout.Label("複数選択したオブジェクトのバウンディングボックス", EditorStyles.boldLabel);

        if (GUILayout.Button("BoxColliderを新規作成"))
        {
            CreateBoxFromMultipleSelection();
        }
    }

    private void CreateBoxFromMultipleSelection()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("1つ以上のオブジェクトを選択してください。");
            return;
        }

        Renderer[] allRenderers = selectedObjects
            .SelectMany(go => go.GetComponentsInChildren<Renderer>())
            .ToArray();

        if (allRenderers.Length == 0)
        {
            Debug.LogWarning("選択したオブジェクトにRendererが見つかりません。");
            return;
        }

        // 合成バウンディングボックス
        Bounds mergedBounds = allRenderers[0].bounds;
        for (int i = 1; i < allRenderers.Length; i++)
        {
            mergedBounds.Encapsulate(allRenderers[i].bounds);
        }

        // Prefab編集モード中かどうかを確認
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();

        GameObject parent = null;
        if (stage != null)
        {
            parent = stage.prefabContentsRoot;
        }

        // 新規作成
        GameObject boxObj = new GameObject("BoxCollider_MergedBounds");
        Undo.RegisterCreatedObjectUndo(boxObj, "Create Merged BoxCollider");

        // 位置と親設定
        boxObj.transform.position = mergedBounds.center;
        boxObj.transform.rotation = Quaternion.identity;
        boxObj.transform.localScale = Vector3.one;

        if (parent != null)
        {
            boxObj.transform.SetParent(parent.transform, true);
        }

        // BoxCollider追加
        BoxCollider boxCollider = boxObj.AddComponent<BoxCollider>();
        boxCollider.center = Vector3.zero;
        boxCollider.size = mergedBounds.size;

        // 選択 & シーンビューにフォーカス
        Selection.activeGameObject = boxObj;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log($"BoxColliderを作成しました（Prefab内: {(parent != null ? "Yes" : "No")}）: {boxObj.name}");
    }
}
