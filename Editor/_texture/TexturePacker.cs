using UnityEngine;
using UnityEditor;
using System.IO;

public class TexturePacker : EditorWindow
{
    [MenuItem("Assets/Convert MRO to ORM")]
    public static void ConvertMROtoORM()
    {
        Texture2D source = Selection.activeObject as Texture2D;
        if (source == null) return;

        // テクスチャを読み取り可能にする設定が必要
        string path = AssetDatabase.GetAssetPath(source);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        importer.SaveAndReimport();

        Color[] pixels = source.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            // MRO (R=Met, G=Rough, B=Occ) -> ORM (R=Occ, G=Rough, B=Met)
            newPixels[i] = new Color(pixels[i].b, pixels[i].g, pixels[i].r, 1.0f);
        }

        Texture2D result = new Texture2D(source.width, source.height);
        result.SetPixels(newPixels);
        result.Apply();

        byte[] bytes = result.EncodeToPNG();
        File.WriteAllBytes(path.Replace(".png", "_ORM.png"), bytes);
        AssetDatabase.Refresh();

        Debug.Log("ORM Texture Generated!");
    }
}