using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class MS_DeepCopyAssets
{

   

    /// <summary>
    /// YAML形式のEncoding
    /// </summary>
    private static Encoding Encoding { get { return Encoding.GetEncoding("UTF-8"); } }

    /// <summary>
    /// コピー後のファイル名やAssetBundle名につける接尾辞
    /// </summary>
    private const string Suffix = "_mod";

    [MenuItem("MS_Tools/Assets/MS_DeepCopyAssets")]
    public static void Execute()
    {
        if (Selection.assetGUIDs.Count() == 0)
        {
            Debug.LogError("コピーしたいアセットを選択してください");
            return;
        }
        DeepCopy(Selection.assetGUIDs);
    }

    /// <summary>
    /// コピー後のパスを返すメソッド
    /// </summary>
    public static string GetCopyPath(string assetPath, string suffix)
    {
        //var directory = System.IO.Path.GetDirectoryName(assetPath);
        var directory = "Assets/WorkSpace/_DeepCopyAssets";
        CreateDeepFolder(directory);
        var filename = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        var extension = System.IO.Path.GetExtension(assetPath);
        return string.Format("{0}/{1}{2}{3}", directory, filename, suffix, extension);
    }

    /// <summary>
    /// 指定フォルダが無かったら作る
    /// </summary>

    public static void CreateDeepFolder(string path)
    {
        // Directory.CreateDirectory は標準の C# 機能
        // これで物理的にフォルダを作ったあと、Unityに再読み込みさせる
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh(); // Unity側に反映させる
            Debug.Log($"一括作成しました: {path}");
        }
    }

    /// <summary>
    /// コピー後のAssetBundle名を返すメソッド
    /// </summary>
    public static string GetCopyAssetBundleName(string assetBundleName, string suffix)
    {
        return Regex.Replace(assetBundleName, @"(?<path>.+)(?<extension>\..+)", string.Format("${{path}}{0}${{extension}}", Suffix));
    }

    /// <summary>
    /// 指定したアセット達をAssetDatabase.ImportAssetする
    /// </summary>
    public static void ImportAssets(IEnumerable<string> assetPaths)
    {
        foreach (var assetPath in assetPaths)
            AssetDatabase.ImportAsset(assetPath);
    }

    //filter mod by MS
    public static string[] DeepCopyFilter(string[] allAssetPaths)
    {
        List<string> newAssetPaths = new List<string>();
        foreach (var assetPath in allAssetPaths)
        {

            //Shaderははじく
            if (AssetDatabase.LoadAssetAtPath<Shader>(assetPath) != null)
            {
                continue;
            }




            // アセットがMaterialかどうかを判定
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material)
            {
                newAssetPaths.Add(assetPath);
                continue;
            }

            // アセットがTextureかどうかを判定
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            if (texture)
            {
                newAssetPaths.Add(assetPath);
                continue;
            }

            // アセットがMeshかどうかを判定
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh)
            {
                newAssetPaths.Add(assetPath);
                continue;
            }

            // アセットがPrefabかどうかを判定
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                newAssetPaths.Add(assetPath);
                continue;
            }
        }
        return newAssetPaths.ToArray();
    }

    /// <summary>
    /// 対象のアセットと依存関係にある全てのアセットをコピーし、それぞれの参照先をコピー先のアセットにする
    /// </summary>
    public static void DeepCopy(string[] targetAssetGUIDs)
    {
        // 対象のアセットのパスを取得
        var targetAssetPaths = targetAssetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid));

        // 対象のアセットと依存関係のある全てのアセットのパスを取得
        var allAssetPaths = AssetDatabase.GetDependencies(targetAssetPaths.ToArray(), true);

        //filter mod by MS
        allAssetPaths = DeepCopyFilter(allAssetPaths);


        // 確認ダイアログを出す
        var assetsCount = allAssetPaths.Count();

        bool confirm = EditorUtility.DisplayDialog(
            "確認",
            string.Format("以下の{0}個のアセットをコピーします。よろしいですか？\n{1}", assetsCount, string.Join("\n", allAssetPaths)),
            "OK",
            "キャンセル"
        );

        if (confirm)
            Debug.Log(string.Format("[Deep Copy Assets] Start...target assets count: {0}", assetsCount));
        else
            return;

        // コピー前のパスとGUIDの対応関係
        var assetPathGUIDMap = allAssetPaths.ToDictionary(path => path, path => AssetDatabase.AssetPathToGUID(path));

        // コピー前後のパスの対応関係
        var assetPathMap = allAssetPaths.ToDictionary(path => path, path => GetCopyPath(path, Suffix));

        // コピー前後のGUIDの対応関係
        var GUIDMap = new Dictionary<string, string>();

        // アセットのコピーを行い、コピー先のGUIDを生成する
        foreach (var kvp in assetPathMap)
        {
            var assetPath = kvp.Key;

            var copyPath = kvp.Value;


            var originalGUID = assetPathGUIDMap[assetPath];

            UnityEngine.Object assetCheck = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(copyPath);
            // アセットのコピーを行う
            if (assetCheck == null)
            {
                System.IO.File.Copy(assetPath, copyPath);
            }
            

            // 一旦ImportしてUnityにGUIDを生成させる
            AssetDatabase.ImportAsset(copyPath);
            var copyGUID = AssetDatabase.AssetPathToGUID(copyPath);
            GUIDMap.Add(originalGUID, copyGUID);

            // メタファイルをGUID、AssetBundle名を書き換えてコピーする
            using (StreamReader sr = new StreamReader(string.Format("{0}.meta", assetPath), Encoding))
            {
                string s = sr.ReadToEnd();
                // GUIDを置換
                s = s.Replace(originalGUID, copyGUID);
                var originalAssetBundleName = Regex.Match(s, @"assetBundleName: (?<path>.+)").Groups["path"].Value;
                if (originalAssetBundleName != "")
                {
                    var copyAssetBundleName = GetCopyAssetBundleName(originalAssetBundleName, Suffix);
                    s = s.Replace(originalAssetBundleName, copyAssetBundleName);
                }

                using (StreamWriter sw = new StreamWriter(string.Format("{0}.meta", copyPath), false, Encoding))
                {
                    sw.Write(s);
                }
            }

            Debug.Log(string.Format("[Copy Asset] from:{0}, to:{1}", assetPath, copyPath));
        }

        // アセットの再読み込みを行う
        // 書き換え直後にImportすると上手く読み込んでくれないことがあるので一通り処理した後に回している
        ImportAssets(assetPathMap.Values);

        foreach (var kvp in assetPathMap)
        {
            var assetPath = kvp.Key;
            var copyPath = kvp.Value;

            // prefabやanimationが参照するGUIDの書き換えを行う
            using (StreamReader sr = new StreamReader(assetPath, Encoding))
            {
                string s = sr.ReadToEnd();
                // YAML形式の場合のみ参照先のGUIDの書き換え処理
                if (s.StartsWith("%YAML"))
                {
                    foreach (var originalAssetPath in assetPathMap.Keys)
                    {
                        var originalAssetGUID = assetPathGUIDMap[originalAssetPath];
                        var copyAssetGUID = GUIDMap[originalAssetGUID];
                        s = s.Replace(originalAssetGUID, copyAssetGUID);
                    }

                    Debug.Log(string.Format("[Replace Dependencies] {0}", copyPath));
                    using (StreamWriter sw = new StreamWriter(copyPath, false, Encoding))
                    {
                        sw.Write(s);
                    }
                }
            }
        }

        // 再読み込みを走らせる（ImportAssetだと上手くいかない）
        AssetDatabase.Refresh();
    }
}