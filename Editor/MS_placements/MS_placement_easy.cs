using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MS_tools;
using MS_tools.lib;
//using log4net.Util;

public class MS_placement_easy : EditorWindow
{

    //Swapmode shortcut key
    public KeyCode swapKey = KeyCode.LeftControl;
    bool swapMode = false;

    //ボタン機能の初期値
    int nameButtonMode = 0;
    int placementMode = 0;

    //作成後のオブジェクト選択モード
    bool selectionMode = true;

    //親オブジェクト
    string parentObjectName;

    //ガイドのアクティベーション
    bool isGuideActive = false;

    //スクロールレイアウト用
    Vector2 scrollPosition = Vector2.zero;


    public Object go;

    [MenuItem("MS_Tools/MS_placement_easy")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_placement_easy));
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        isGuideActive = false;
    }
    
    

    void OnGUI()
    {
        
        // ===== Top =====
        
        // 設定


        //MS_PlacementSO
        EditorGUILayout.LabelField("MS_PlacementSOs");

        //　アセット名ボタンコマンド選択
        string[] nameButtonModeLabel = { "Select Origin", "ScreenShot：スクリーンショット撮影" };
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("NameButtonCommand");
        nameButtonMode = (int)EditorGUILayout.Popup(nameButtonMode, nameButtonModeLabel);
        EditorGUILayout.EndHorizontal();

        //　サムネイルコマンド選択
        string[] mode = { 
            "Simple Raycast",
            "Selection:選択オブジェクトの情報を利用"
        };
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ThumbnailCommand");
        placementMode = (int)EditorGUILayout.Popup(placementMode, mode);
        EditorGUILayout.EndHorizontal();

        //選択モード
        selectionMode = (bool)EditorGUILayout.ToggleLeft("Selection New : 新しく作ったオブジェクトを選択する",selectionMode);

        //親子モード
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("parent：指定文字列のオブジェクトの子に作成");
        parentObjectName = EditorGUILayout.TextField("stage");
        EditorGUILayout.EndHorizontal();

        //ガイド表示
        isGuideActive = (bool)EditorGUILayout.ToggleLeft("isGuideActive",isGuideActive);




        // ===== Middle =====================================================================================

        //
        EditorGUILayout.LabelField("プレファブ登録");
        go = EditorGUILayout.ObjectField(go, typeof(object), true);

        // スクロールビューを作成
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);



        // Buttons
        int x = 0;
        int buttonSize = 100;

        if (swapMode)
        {
            GUI.backgroundColor = new Color(0.9f, 0.7f, 0.6f);
        }

        GUILayout.BeginHorizontal();



        // ===== Start ToolChip ======

        //Debug.Log(go.GetType());
        if (go.GetType() == typeof(DefaultAsset))
        {
            Debug.Log(go.ToString());
        }
        else if (go)
        {
            //　アセット名ボタン
            if (GUILayout.Button(go.name, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize * 0.2f)))
            {
                switch (nameButtonMode)
                {
                    case 0:
                        Debug.Log(go.name);
                        Selection.activeObject = go;
                        break;
                    case 1:
                        break;
                }
            }

            if (GUILayout.Button("placement", GUILayout.Width(buttonSize), GUILayout.Height(buttonSize * 0.2f)))
            {
                switch (placementMode)
                {
                    case 0:
                        PlacementSimpleRaycast(go as GameObject);
                        break;
                    case 1:
                        //swapMode = false;
                        PlacementBySelection(go as GameObject);
                        break;
                }
            }


            // ===== End ToolChip =============================================================================




            //
            x += buttonSize;
            if (x > (position.size.x - 50))
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                x = 0;
            }
        }


        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        //　===== Bottom =====

        //　Discriptions
        EditorGUILayout.LabelField("swapmode shortcutkey = " + swapKey + " （Swapはどのモードでも効きます）");


        //Swapモードフラグ
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == swapKey)
        {
            swapMode = true;
        }
        if (e.type == EventType.KeyUp && e.keyCode == swapKey)
        {
            swapMode = false;
        }
    }


    /// <summary>
    /// GameObjectインスタンス生成関連機能
    /// </summary>
    /// <param name="go"></param>
    /// <param name="sibilingIndex"></param>
    /// <returns></returns>
    GameObject CreateNewGameObject(GameObject go, int sibilingIndex = 99999999)
    {
        // インスタンス生成
        GameObject newGO = (GameObject)PrefabUtility.InstantiatePrefab(go);
        Undo.RegisterCreatedObjectUndo(newGO, "ngo");
        // HierarchyViewの一番下にもってくる
        newGO.transform.SetSiblingIndex(sibilingIndex);

        return newGO;
    }

    /// <summary>
    /// レイキャスト配置
    /// </summary>
    /// <param name="go"></param>
    /// <param name="selectionMode"></param>
    void PlacementSimpleRaycast(GameObject go)
    {
        
        if (swapMode)
        {
            PlacementBySelection(go);
            return;
        }
        GameObject newGO = CreateNewGameObject(go);

        newGO.transform.position = GetRaycastHitPoint();

        //親の下に配置
                
        if (GameObject.Find(parentObjectName))
        {
            Transform parentObject = GameObject.Find(parentObjectName).transform;
            newGO.transform.parent = parentObject; 
        }
        else if(parentObjectName!=null)
        {
            Transform parentNew = new GameObject(parentObjectName).transform;
            newGO.transform.parent = parentNew;
        }

        //selectionモードOnのとき
        if (selectionMode) { Selection.activeGameObject = newGO; }
    }

    /// <summary>
    /// カメラからのRayがHitしたPointを返す
    /// </summary>
    /// <returns></returns>
    Vector3 GetRaycastHitPoint()
    {
        Vector3 ret = Vector3.zero;

        var sceneCamera = SceneView.lastActiveSceneView.camera;
        var worldRay = sceneCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        var ray = new Ray(worldRay.origin, worldRay.direction);

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 5);//Rayのデバッグドロー

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != null)
            {
                ret = hit.point;
            }
        }

        return ret;
    }

    /// <summary>
    /// 選択オブジェクトの情報を基に配置
    /// </summary>
    /// <param name="go"></param>
    void PlacementBySelection(GameObject go)
    {
        GameObject[] selectedGameOblects = Selection.gameObjects;
        
        List<GameObject> newGameObjects = new List<GameObject>();

        if (selectedGameOblects.Length <= 0)    //何も選んでないとき
        {
            GameObject newGO = CreateNewGameObject(go);


            //親の下に配置
            Transform parentObject = GameObject.Find(parentObjectName).transform;
            if (parentObject) { newGO.transform.parent = parentObject; }

            //selectionモードOnのとき
            if (selectionMode) { Selection.activeGameObject = newGO; }

        }
        else
        {
            foreach (GameObject selectedGO in selectedGameOblects)  //選択時
            {
                GameObject newGO = CreateNewGameObject(go);
                Undo.RegisterCreatedObjectUndo(newGO, "ngo");

                newGO.transform.position = selectedGO.transform.position;
                newGO.transform.rotation = selectedGO.transform.rotation;
                newGO.transform.localScale = selectedGO.transform.localScale;


                //親の下に配置
                newGO.transform.parent = selectedGO.transform.parent;

                // SwapモードOnのとき
                if (swapMode) { 
                    //DestroyImmediate(selectedGO);
                    Undo.DestroyObjectImmediate(selectedGO);

                }
                

                // selectionモードOnのとき
                newGameObjects.Add(newGO);

            }
            if (selectionMode) {
                GameObject[] newGOs = newGameObjects.ToArray();
                 Selection.objects = newGOs;
            }
        }
    }



    /// <summary>
    /// スクリーンショットを撮る
    /// </summary>
    /// <param name="filePath"></param>
    void ScreenShot(string filePath)
    {

        if (!string.IsNullOrEmpty(filePath))
        {
            // SceneViewをアクティブにする
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.Focus();

                // シーンビューを描画する
                SceneView.RepaintAll();

                // スクリーンショットを撮る
                int TextureSize = 512;
                RenderTexture rt = new RenderTexture(TextureSize, TextureSize, 24);
                Texture2D screenshot = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
                sceneView.camera.targetTexture = rt;
                sceneView.camera.Render();
                RenderTexture.active = rt;
                screenshot.ReadPixels(new Rect(0, 0, TextureSize, TextureSize), 0, 0);
                screenshot.Apply();
                byte[] bytes = screenshot.EncodeToPNG();
                DestroyImmediate(screenshot);

                // ファイルを保存する
                System.IO.File.WriteAllBytes(filePath, bytes);
                AssetDatabase.ImportAsset(filePath);
                // コンソールに保存したファイルのパスを表示する
                Debug.Log($"Screenshot saved to {filePath}");
            }
        }
    }


    /// <summary>
    /// ガイド表示
    /// </summary>
    /// <param name="sceneView"></param>
    private void OnSceneGUI(SceneView sceneView)
    {
        if (isGuideActive)
        {
            Handles.BeginGUI();

            Vector2 screenCenter = new Vector2(sceneView.camera.pixelWidth * 0.5f, sceneView.camera.pixelHeight * 0.5f);
            Handles.color = new Color(1.0f, 1.0f, 1.0f);

            //センターガイド
            Handles.DrawSolidDisc(screenCenter, Vector3.forward, 2f);

            //スクリーンショットガイド
            float size = screenCenter.y; // ワイヤーフレームのサイズ
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(screenCenter.x - size, screenCenter.y - size, 0),
                new Vector3(screenCenter.x + size, screenCenter.y - size, 0),
                new Vector3(screenCenter.x + size, screenCenter.y + size, 0),
                new Vector3(screenCenter.x - size, screenCenter.y + size, 0)
            };

            Handles.DrawLine(vertices[0], vertices[1]);
            Handles.DrawLine(vertices[1], vertices[2]);
            Handles.DrawLine(vertices[2], vertices[3]);
            Handles.DrawLine(vertices[3], vertices[0]);

            Handles.EndGUI();
        }
    }


}

