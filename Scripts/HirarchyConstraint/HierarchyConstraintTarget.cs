using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 階層コンストレイン
/// sourceの階層オブジェクトをtarget階層にコンストレインする
/// </summary>
/// 
[System.Serializable]
public class HierarchyConstraintTarget : MonoBehaviour
{
    [System.Serializable]
    public class PairTransform
    {
        public Transform target;
        public Transform source;
    }

    [SerializeField] public PairTransform pairRoot;

    [SerializeField] public Vector3 offsetRoot;
    [SerializeField] public Vector3 offsetRootRotation;

    public HierarchyConstraintSource hcs;



    List<PairTransform> pairs;
    Transform root;

    private void Awake()
    {
        pairs = new List<PairTransform>();

        AddPairsRecursive(pairRoot.source , pairRoot.target);
    }

    // Update is called once per frame
    void Update()
    {
        if (pairRoot.source != null && pairRoot.target != null)
        {
            //ポジションオフセット
            root.position = pairs[0].target.position;
            root.rotation = pairs[0].target.rotation;


            pairs[0].source.localPosition = offsetRoot;
            pairs[0].source.rotation *= Quaternion.Euler(offsetRootRotation);

            foreach (PairTransform pair in pairs)
            {
                pair.source.rotation = pair.target.rotation * Quaternion.Euler(offsetRootRotation);
            }
        }
    }
    // ジョイントペアリング
    void AddPairsRecursive(Transform source, Transform target)
    {
        if (source == null || target == null)
            return;

        // この階層のペアを登録
        pairs.Add(new PairTransform
        {
            source = source,
            target = target
        });

        // 子の数が一致していない場合は安全のため最小数で回す
        int childCount = Mathf.Min(source.childCount, target.childCount);

        for (int i = 0; i < childCount; i++)
        {
            AddPairsRecursive(
                source.GetChild(i),
                target.GetChild(i)
            );
        }

        // ルート定義
        root = pairs[0].source.parent;
        //ポジションオフセット
        root.position = pairs[0].target.position;
        root.rotation = pairs[0].target.rotation;

    }

    // 実行時の再割り付け用
    public void RebuildPairs()
    {
        pairs = new List<PairTransform>();

        if (pairRoot.source == null || pairRoot.target == null)
            return;

        AddPairsRecursive(pairRoot.source, pairRoot.target);

        Debug.Log($"Rebuild pairs : {pairs.Count}");
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        RebuildPairs();
    }
#endif

}
