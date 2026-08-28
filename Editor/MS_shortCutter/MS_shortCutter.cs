using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// フォルダーのショートカット
/// </summary>
public class MS_shortCutter : EditorWindow
{
    [System.Serializable]
    public class ShortCut
    {
        public string name;
        public string sceneAsset;
    }

    [System.Serializable]
    public class ShortCutList
    {
        public ShortCutList() { shortCuts = new List<ShortCut>(); }
        public List<ShortCut> shortCuts;
    }


    public ShortCutList shortCutList = new ShortCutList();

    string saveFilePath = GetThisPath()+ "/MS_shortCutter_settings.json";

    //addMode
    KeyCode addKey = KeyCode.LeftShift;
    bool addMode = false;

    // 位置保存用フィールド
    Vector2 contextScreenPos;

    [MenuItem("MS_Tools/MS_shortCutter")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MS_shortCutter));
    }


    void OnGUI()
    {

        var e = Event.current;
        // シーン追加用
        if (e.type == EventType.KeyDown && e.keyCode == addKey)
        {
            addMode = true;
        }
        if (e.type == EventType.KeyUp && e.keyCode == addKey)
        {
            addMode = false;
        }
        if (addMode)
        {
            GUI.backgroundColor = new Color(0.7f, 0.9f, 0.7f);
        }

        EditorGUILayout.LabelField("scenes");
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.LabelField("Scenes (Shift = addMode)");
        foreach (var folder in shortCutList.shortCuts)
        {
            if (folder.sceneAsset != null)
            {
                // 右クリック用
                Rect rect = GUILayoutUtility.GetRect(new GUIContent("Right Click Me"),GUI.skin.button,GUILayout.Height(30));

                // ★ 先に右クリックを捕まえる ★
                if (e.type == EventType.MouseDown && e.button == 1 &&
                    rect.Contains(e.mousePosition))
                {
                    var capturedFolder = folder;
                    // ★ GUI座標 → Screen座標へ変換して保存
                    contextScreenPos = GUIUtility.GUIToScreenPoint(e.mousePosition);

                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Delete"), false, () =>
                    {
                        RemoveShortCut(capturedFolder);
                    });
                    menu.AddItem(new GUIContent("Rename"), false, () =>
                    {
                        RenameShortCut(capturedFolder);
                    });
                    menu.ShowAsContext();
                    e.Use();
                }

                // 
                if (GUI.Button(rect,folder.name))
                {

                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {

                        if (!addMode)
                        {
                            EditorSceneManager.OpenScene(folder.sceneAsset);
                        }
                        else
                        {
                            EditorSceneManager.OpenScene(folder.sceneAsset, OpenSceneMode.Additive);
                        }
                    }
                }
            }
        }

        // D&Dを受け付ける領域を定義する (例: 高さ100pxのスペース)
        Rect dropArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "ここにアセットをドラッグ＆ドロップ");

        // D&Dイベントを処理するメソッドを呼び出す
        HandleDragAndDrop(dropArea);
        
        EditorGUILayout.EndVertical();
    }

    void OnEnable()
    {
        // ウィンドウが開いたときにデータを読み込む
        shortCutList = LoadFolderPaths();
    }

    private void OnDisable()
    {
        // 3. System.IO を使ってファイルに書き込む
        try
        {
            var jsonString = JsonUtility.ToJson(shortCutList, true);
            File.WriteAllText(saveFilePath, jsonString);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("JSONファイルの保存に失敗しました: " + e.Message);
        }
    }


    // JSONデータをファイルから読み込んで、リストを返すメソッド
    public ShortCutList LoadFolderPaths()
    {
        if (File.Exists(saveFilePath))
        {
            ShortCutList list = new ShortCutList();
            try
            {
                // 1. ファイルからJSON文字列をすべて読み込む
                string jsonContent = File.ReadAllText(saveFilePath);

                // 2. JsonUtility.FromJson<T>() で文字列をクラスインスタンスに変換
                // <T> の部分には復元したい型 (FolderListWrapper) を指定する
                var temp = JsonUtility.FromJson<ShortCutList>(jsonContent);
                foreach (var item in temp.shortCuts)
                {
                    list.shortCuts.Add(item);
                }

                if (list != null)
                {
                    UnityEngine.Debug.Log("JSONデータのロードに成功しました。フォルダ数: " + list.shortCuts.Count);
                    return list; // 復元されたリストを返す
                }
                else
                {
                    UnityEngine.Debug.LogError("JSONデータのデシリアライズに失敗しました。");
                    return new ShortCutList();
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("JSONファイルの読み込み中にエラーが発生しました: " + e.Message);
                return new ShortCutList();
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("保存ファイルが見つかりません: " + saveFilePath);
            return new ShortCutList(); // ファイルがない場合は空のリストを返す
        }
    }


    // Drag&Drop Core
    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;

        // dropArea内でのイベントのみを処理対象とする
        if (!dropArea.Contains(evt.mousePosition)) return;

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                // マウスイベントを消費し、Unity EditorにD&Dを処理中であることを伝える
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag(); // ドロップ操作を確定する

                    // ドロップされた全てのアセットを処理
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        // DefaultAsset型に絞り込む（フォルダや不明なファイル形式）
                        if (draggedObject is SceneAsset sce)
                        {
                            var newSCF = new ShortCut();
                            newSCF.name = sce.name;
                            newSCF.sceneAsset = AssetDatabase.GetAssetPath(sce);
                            shortCutList.shortCuts.Add(newSCF);
                            
                        }
                        // 他の型 (例: GameObject, Texture) を受け入れたい場合はここに追加
                    }
                    // リストが更新されたことをEditorに通知して再描画させる
                    GUI.changed = true;
                }
                break;
            case EventType.DragExited:
                // エリア外に出たときの処理（必要なら）
                break;
        }
    }

    void RemoveShortCut(ShortCut target)
    {
        if (shortCutList.shortCuts.Remove(target))
        {
            GUI.changed = true;
            Repaint();
        }
    }

    void RenameShortCut(ShortCut target)
    {
        if (target == null) return;

        Rect rect = new Rect(
            GUIUtility.GUIToScreenPoint(contextScreenPos),
            Vector2.zero);

        TextInputPopup.ShowAt(contextScreenPos, "名前を入力", (input) =>
        {
            target.name = input;
            Repaint();
        });
    }

    // ===========================================
    // スクリプト実行パス取得
    // ===========================================
    public static string GetThisPath()
    {
        // 現在実行中のメソッドを取得
        StackTrace stackTrace = new StackTrace(true);
        StackFrame frame = stackTrace.GetFrame(0);
        string absolutePath = frame.GetFileName(); // OSの絶対パスを取得

        // Unityのプロジェクト相対パスに変換したい場合
        string relativePath = GetUnityAssetPath(absolutePath);
        var relativePaths = relativePath.Split("/");
        relativePath = string.Join("/", relativePaths.Take(relativePaths.Length-1));
        return relativePath;

    }
    private static string GetUnityAssetPath(string fullPath)
    {
        if (fullPath.Contains("/Assets/")||fullPath.Contains("\\Assets\\"))
        {
            // WindowsとMac/Linuxの両方に対応するため、スラッシュで統一して変換
            string unityPath = fullPath.Replace('\\', '/');
            int assetsIndex = unityPath.IndexOf("/Assets/");
            // "Assets/" から始まる相対パスを返す
            return unityPath.Substring(assetsIndex + 1);
        }
        return fullPath; // Assetsフォルダ以下にない場合はそのまま返す
    }
}
