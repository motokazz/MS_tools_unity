using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MS_tools.lib
{
    public class PathModules
    {
        /// <summary>
        /// DefaultAssetからディレクトリのパスを取得する
        /// </summary>
        public static string GetDirectoryPathByDefaultAsset(DefaultAsset defaultAsset)
        {
            if (defaultAsset == null) return null;

            // DefaultAssetのパスを取得する
            string path = AssetDatabase.GetAssetPath(defaultAsset);
            if (string.IsNullOrEmpty(path)) return null;

            // 取得したパスがディレクトリのパスの時だけ、パスを返す
            bool isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            if (isDirectory == false) return null;
            return path;
        }

        /// <summary>
        /// 関数を呼び出したスクリプトが置かれているフルパスを返す
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = null)
        {
            return filePath.Replace(System.IO.Path.DirectorySeparatorChar, '/').Replace(Application.dataPath, "Assets");
        }

    }

}
