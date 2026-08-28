using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static PlasticGui.PlasticTableColumn;

public class MS_curringSelect : EditorWindow
{

    List<GameObject> result = new List<GameObject>();
    GameObject temp;

    [MenuItem("MS_Tools/Level/MS_curringSelect")]

    [ExecuteInEditMode]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_curringSelect));

    }
	void OnGUI()
	{

		// EditorGUILayoutの使用例.
		EditorGUILayout.LabelField("MS_curringSelect");

        // 実行ボタン.
        if (GUI.Button(new Rect(0.0f, 80.0f, 120.0f, 20.0f), "Frustum"))
        {
            Frustum();
        }		// 実行ボタン.
		if (GUI.Button(new Rect(0.0f, 100.0f, 120.0f, 20.0f), "Rendered"))
		{
            Rendered();
		}
        // 実行ボタン.
        if (GUI.Button(new Rect(0.0f, 120.0f, 120.0f, 20.0f), "OnWillRenderObject"))
        {
            OnWillRenderObject();
        }

        // 実行ボタン.
        if (GUI.Button(new Rect(0.0f, 160.0f, 120.0f, 20.0f), "StoreResult"))
        {
            StoreResult();
        }
        // 実行ボタン.
        if (GUI.Button(new Rect(0.0f, 180.0f, 120.0f, 20.0f), "SelectResult"))
        {
            SelectResult();
        }

        // 実行ボタン.
        if (GUI.Button(new Rect(0.0f, 200.0f, 120.0f, 20.0f), "CheckPrefab"))
        {
            //CheckPrefab();
        }

        void Frustum()
        {
            result = new List<GameObject>();
            Camera mainCamera = Camera.main;

            // カメラの視錐台を取得
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            // シーン内の全オブジェクトを検索
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                // 視錐台内にあるか判定
                if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                {
                    // オブジェクトが視界内にある場合の処理
                    if (renderer.isVisible)
                    {

                        CheckPrefab(renderer.gameObject);

                        result.Add(temp);
                    }
                }
            }
            Selection.objects = result.ToArray();
            Debug.Log(result.Count);

        }

        void Rendered()
		{
            result = new List<GameObject>();
            Camera mainCamera = Camera.main;

			foreach(Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
			{
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(renderer.bounds.center);
                // スクリーン内に位置しているかチェック
                if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= Screen.width && screenPoint.y >= 0 && screenPoint.y <= Screen.height)
                {
                    // カメラからレイを飛ばして視界を確認
                    if (!Physics.Linecast(mainCamera.transform.position, renderer.bounds.center))
                    {
                        //Debug.Log($"{renderer.gameObject.name} is visible");
                        result.Add((GameObject)renderer.gameObject);
                    }
                }
            }
            Selection.objects = result.ToArray();
        }

        void OnWillRenderObject()
        {
            result = new List<GameObject>();
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                // オブジェクトが視界内にある場合の処理
                if (renderer.isVisible)
                {
                    result.Add(renderer.gameObject);
                }
            }
            Selection.objects = result.ToArray();
        }

        void StoreResult()
        {
            result.Clear();
            foreach(GameObject gameObject in Selection.gameObjects)
            {
                result.Add(gameObject);
            }
            
        }

        void SelectResult()
        {
            Selection.objects = result.ToArray();
        }

        void CheckPrefab(GameObject gameObject)
        {
            GameObject go = gameObject;
            while (true)
            {
                if (PrefabUtility.GetPrefabAssetType(go.transform.parent) == PrefabAssetType.Regular)
                {
                    go = go.transform.parent.gameObject;
                }
                else
                {
                    temp = go;
                    break;
                }
            }
        }
    }
}

