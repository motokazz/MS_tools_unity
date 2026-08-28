using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 階層コンストレイン
/// sourceの階層オブジェクトをtarget階層にコンストレインする
/// </summary>
/// 
[System.Serializable]
public class HierarchyConstraintSource : MonoBehaviour
{
    [SerializeField] public Transform source;
    [SerializeField] public Vector3 offsetRoot;
    [SerializeField] public Vector3 offsetRootRotation;
    [SerializeField] public float offsetTop = 0f;

    [ContextMenu("change")]
    void changeValue()
    {

    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        changeValue();
    }
#endif
}
