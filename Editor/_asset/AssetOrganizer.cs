using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AssetOrganizer : EditorWindow
{
    [MenuItem("MS_Tools/Assets/タイプ別フォルダに整理")]
    public static void Organize()
    {
        // 選択されたオブジェクトを取得
        Object[] selectedAssets = Selection.objects;

        if (selectedAssets.Length == 0)
        {
            Debug.LogWarning("整理するアセットが選択されていません。");
            return;
        }

        foreach (var asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);

            // フォルダ自体はスキップ
            if (AssetDatabase.IsValidFolder(assetPath)) continue;

            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileName(assetPath);
            string folderName = GetFolderName(asset);

            if (string.IsNullOrEmpty(folderName)) continue;

            // 移動先フルパス
            string destinationFolder = Path.Combine(directory, folderName);
            string destinationPath = Path.Combine(destinationFolder, fileName);

            // フォルダが存在しない場合は作成
            if (!AssetDatabase.IsValidFolder(destinationFolder))
            {
                AssetDatabase.CreateFolder(directory, folderName);
            }

            // アセットの移動（Undo可能にする）
            string error = AssetDatabase.MoveAsset(assetPath, destinationPath);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Error moving {fileName}: {error}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("アセットの整理が完了しました！");
    }

    private static string GetFolderName(Object asset)
    {
        // 拡張子または型でフォルダ名を判定
        if (asset is Material) return "Materials";
        if (asset is GameObject)
        {
            // PrefabかModel(FBX)かを判定
            string path = AssetDatabase.GetAssetPath(asset);
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".fbx" || ext == ".obj") return "Models";
            return "Prefabs";
        }
        if (asset is Texture) return "Textures";
        if (asset is RuntimeAnimatorController) return "Animators";
        if (asset is AnimationClip) return "Motions";

        // Timelineなどの特殊な型への対応
        string typeName = asset.GetType().Name;
        if (typeName == "TimelineAsset") return "Timelines";

        return null; // 該当なし
    }
}