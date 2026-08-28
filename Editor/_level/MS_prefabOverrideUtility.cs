using UnityEditor;
using UnityEngine;

public class PrefabOverrideUtility
{
    [MenuItem("MS_Tools/Level/Force Transform Override %#o")]
    private static void ForceTransformOverride()
    {
        var selectedObjects = Selection.transforms;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていません。");
            return;
        }

        foreach (var t in selectedObjects)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(t))
            {
                Debug.Log($"{t.name} はPrefabインスタンスではありません。");
                continue;
            }

            Undo.RecordObject(t, "Force Transform Override");

            // 元の値を保持
            Vector3 originalPosition = t.localPosition;
            Quaternion originalRotation = t.localRotation;
            Vector3 originalScale = t.localScale;

            // 1ピクセル分だけ位置をずらす
            t.localPosition += Vector3.right * 0.001f;
            t.localScale += new Vector3(1, 1, 1);
            // Dirty フラグを立てて変更を検知させる
            EditorUtility.SetDirty(t);

            // 戻す
            t.localPosition = originalPosition;
            t.localRotation = originalRotation;
            t.localScale = originalScale;

            EditorUtility.SetDirty(t);

            // Prefabへのオーバーライドを記録
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);

            Debug.Log($"Transform にオーバーライドを付与: {t.name}");
        }
    }
}
