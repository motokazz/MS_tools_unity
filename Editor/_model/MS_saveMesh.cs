using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ツール等で作成されたメッシュをアセットとして保存するスクリプト
/// MeshFilterコンポーネントのコンテキストメニューからコマンド選択する
/// </summary>
public static class MeshUtil
{
    [MenuItem("CONTEXT/MeshFilter/Save Mesh")]
    private static void SaveMesh(MenuCommand menuCommand)
    {
        var c = menuCommand.context as MeshFilter;
        if (c.sharedMesh == null)
            return;

        var path = EditorUtility.SaveFilePanelInProject("Save Mesh", "Mesh", "asset", "");
        if (string.IsNullOrEmpty(path))
            return;

        var mesh = GameObject.Instantiate(c.sharedMesh);
        var asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (asset != null)
        {
            EditorUtility.CopySerialized(asset, mesh);
        }
        else
        {
            AssetDatabase.CreateAsset(mesh, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
