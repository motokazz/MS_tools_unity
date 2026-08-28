using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MS_MaterialCleaner : Editor
{
    // インスペクター上のマテリアル設定の歯車(または右クリック)から実行
    [MenuItem("CONTEXT/Material/Clear Unused Properties")]
    public static void ClearUnusedProperties(MenuCommand command)
    {
        Material mat = command.context as Material;
        if (mat != null)
        {
            CleanMaterial(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"{mat.name} の不要なプロパティ履歴を削除しました。");
        }
    }

    // Projectビューで複数選択して右クリックから一括実行
    [MenuItem("Assets/MS_MaterialCleaner")]
    public static void CleanSelectedMaterials()
    {
        bool changed = false;
        foreach (Object obj in Selection.objects)
        {
            Material mat = obj as Material;
            if (mat != null)
            {
                CleanMaterial(mat);
                changed = true;
            }
        }
        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("選択されたマテリアルの不要なプロパティ履歴を削除しました。");
        }
    }

    private static void CleanMaterial(Material mat)
    {
        // マテリアルのシリアライズデータを取得
        SerializedObject so = new SerializedObject(mat);
        so.Update();

        SerializedProperty savedProps = so.FindProperty("m_SavedProperties");
        if (savedProps == null) return;

        // 現在のシェーダーが持っている有効なプロパティ名の一覧を取得
        HashSet<string> validProperties = new HashSet<string>();
        int propCount = ShaderUtil.GetPropertyCount(mat.shader);
        for (int i = 0; i < propCount; i++)
        {
            validProperties.Add(ShaderUtil.GetPropertyName(mat.shader, i));
        }

        // テクスチャ、Float、Colorの不要な履歴をそれぞれ掃除
        CleanPropertyArray(savedProps.FindPropertyRelative("m_TexEnvs"), validProperties);
        CleanPropertyArray(savedProps.FindPropertyRelative("m_Floats"), validProperties);
        CleanPropertyArray(savedProps.FindPropertyRelative("m_Colors"), validProperties);

        so.ApplyModifiedProperties();
    }

    private static void CleanPropertyArray(SerializedProperty arrayProp, HashSet<string> validProperties)
    {
        if (arrayProp == null) return;

        // インデックスがずれないように後ろからループして削除
        for (int i = arrayProp.arraySize - 1; i >= 0; i--)
        {
            string propName = arrayProp.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
            
            // 現在のシェーダーに存在しないプロパティなら削除
            if (!validProperties.Contains(propName))
            {
                arrayProp.DeleteArrayElementAtIndex(i);
            }
        }
    }
}