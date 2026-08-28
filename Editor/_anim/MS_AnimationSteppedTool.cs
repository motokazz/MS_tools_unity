using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
/// <summary>
/// アニメーションクリップのコマ落としツール
/// アニメーションクリップをコマ落としして指定アニメーションクリップに上書きする
/// </summary>
public class MS_AnimationSteppedTool : EditorWindow
{
    AnimationClip originalClip;
    AnimationClip newClip;

    [MenuItem("MS_Tools/Anim/MS_AnimationSteppedTool")]
    static void Init()
    {
        MS_AnimationSteppedTool window = (MS_AnimationSteppedTool)EditorWindow.GetWindow(typeof(MS_AnimationSteppedTool));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("MS Stepped Animation Tool", EditorStyles.boldLabel);
        originalClip = EditorGUILayout.ObjectField("Original Animation Clip", originalClip, typeof(AnimationClip), false) as AnimationClip;
        newClip = EditorGUILayout.ObjectField("New Animation Clip", newClip, typeof(AnimationClip), false) as AnimationClip;


        if (GUILayout.Button("15fps"))
        {
            ConvertToSteppedAnimation(15f);
        }
        if (GUILayout.Button("10fps"))
        {
            ConvertToSteppedAnimation(10f);
        }
        if (GUILayout.Button("8fps"))
        {
            ConvertToSteppedAnimation(8f);
        }
        if (GUILayout.Button("5fps"))
        {
            ConvertToSteppedAnimation(5f);
        }
        if (GUILayout.Button("3fps"))
        {
            ConvertToSteppedAnimation(3f);
        }
        if (GUILayout.Button("Origin"))
        {
            TransferClip();
        }
        /*
        if (GUILayout.Button("ClearClip"))
        {
            ClearAnimationClip(newClip);
        }
        if (GUILayout.Button("CheckClip"))
        {
            CheckClip(newClip);
        }
        */
    }

    void ConvertToSteppedAnimation(float targetFPS = 15f)
    {
        if (originalClip == null)
        {
            Debug.LogWarning("Please select an original animation clip.");
            return;
        }

        if (newClip == null)
        {
            Debug.LogWarning("Please specify a new animation clip.");
            return;
        }

        //出力先がFBXだった場合処理しない
        if (CheckClip(newClip))
        {
            Debug.LogWarning("This AnimationClip is FBX. Operation Canceled.");
            return;
        }

        //出力対象のAnimationClipをクリアする
        ClearAnimationClip(newClip);


        float timePerFrame = 1f / targetFPS;

        // アニメーションのキーフレームを取得
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);

        foreach (EditorCurveBinding curveBinding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, curveBinding);

            // 新しいキーフレームのリスト
            AnimationCurve newCurve = new AnimationCurve();

            // 最初のキーフレームを追加
            newCurve.AddKey(curve.keys[0]);

            float currentTime = 0f;
            int nextKeyIndex = 1;

            // キーフレームをステップ化
            while (nextKeyIndex < curve.keys.Length)
            {
                // 次のキーフレームの時間を取得
                float nextKeyTime = curve.keys[nextKeyIndex].time;

                // 次のキーフレームまでの時間が1フレーム分以上ある場合は新しいキーフレームを追加
                while (currentTime + timePerFrame <= nextKeyTime)
                {
                    currentTime += timePerFrame;
                    float nextKeyValue = curve.Evaluate(currentTime);

                    Keyframe newKey = new Keyframe(currentTime, nextKeyValue);

                    newCurve.AddKey(newKey);
                }

                // 次のキーフレームへ
                nextKeyIndex++;
            }

            //最後のキーフレーム追加
            currentTime += timePerFrame;
            float endKeyValue = curve.Evaluate(currentTime);
            Keyframe endKey = new Keyframe(currentTime, endKeyValue);
            newCurve.AddKey(endKey);


            //補完タイプコンスタントに変更
            nextKeyIndex = 0;
            while (nextKeyIndex < newCurve.keys.Length)
            {
                AnimationUtility.SetKeyLeftTangentMode(newCurve, nextKeyIndex, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(newCurve, nextKeyIndex, AnimationUtility.TangentMode.Constant);
                nextKeyIndex++;
            }


            // 新しいアニメーションカーブを適用
            AnimationUtility.SetEditorCurve(newClip, curveBinding, newCurve);
        }

        //AssetDatabase.CreateAsset(newAnimationClip, AssetDatabase.GetAssetPath(newClip));

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip), ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();


        Debug.Log("Animation converted to stepped animation and saved as: " + newClip.name);

    }

    void TransferClip()
    {
        if (originalClip == null)
        {
            Debug.LogWarning("Please select an original animation clip.");
            return;
        }

        if (newClip == null)
        {
            Debug.LogWarning("Please specify a new animation clip.");
            return;
        }

        //出力先がFBXだった場合処理しない
        if (CheckClip(newClip))
        {
            Debug.LogWarning("This AnimationClip is FBX. Operation Canceled.");
            return;
        }

        //出力対象のAnimationClipをクリアする
        ClearAnimationClip(newClip);
        // アニメーションのキーフレームを取得
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
        foreach (EditorCurveBinding curveBinding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, curveBinding);

            // 新しいキーフレームのリスト
            AnimationCurve newCurve = new AnimationCurve();

            // 新しいアニメーションカーブを適用
            AnimationUtility.SetEditorCurve(newClip, curveBinding, curve);
        }
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newClip), ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();


        Debug.Log("Animation converted to stepped animation and saved as: " + newClip.name);

    }

    void ClearAnimationClip(AnimationClip clip)
    {
        // アニメーションのキーフレームを取得
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

        // 各カーブを削除する
        foreach (EditorCurveBinding curveBinding in curveBindings)
        {
            AnimationUtility.SetEditorCurve(clip, curveBinding, null);
        }

        // アニメーションクリップの長さを0に設定する
        clip.frameRate = 0f;
        //clip.ClearCurves(); // 非推奨
    }

    bool CheckClip(AnimationClip clip)
    {
        string assetPath = AssetDatabase.GetAssetPath(clip);
        if (assetPath.ToLower().EndsWith(".fbx"))
        {
            Debug.Log("Asset is import with FBX");
            return true;
            // アニメーションクリップはFBXとしてインポートされました
        }
        else
        {
            Debug.Log("Asset is Unique AnimationClip");
            return false;
        }
    }
}
