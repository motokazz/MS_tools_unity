using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HierarchyConstraintManager : MonoBehaviour
{

    [SerializeField] int targetID = 0;
    [SerializeField] List<HierarchyConstraintTarget> HCTs;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] float range=1;
    [SerializeField] Transform topTransform;



    private InputAction inputAction1;
    private InputAction inputAction2;
    private InputAction inputAction3;
    private InputAction inputAction4;

    HierarchyConstraintSource currentHCS;
    HierarchyConstraintSource[] currentHCSs;

    private void Awake()
    {
        inputAction1 = playerInput.actions.FindAction("Action1");
        inputAction2 = playerInput.actions.FindAction("Action2");
        inputAction3 = playerInput.actions.FindAction("Action3");
        inputAction4 = playerInput.actions.FindAction("Action4");
    }

    void OnEnable()
    {
        // 2. イベントにデリゲートを登録（+=）
        inputAction1.performed += OnAction1Performed;
        inputAction2.performed += OnAction2Performed;
        inputAction3.performed += OnAction3Performed;
        inputAction4.performed += OnAction4Performed;
    }

    void OnDisable()
    {
        // 3. 無効化するときは必ず解除する（-=）
        inputAction1.performed -= OnAction1Performed;
        inputAction2.performed -= OnAction2Performed;
        inputAction3.performed -= OnAction3Performed;
        inputAction4.performed -= OnAction4Performed;
    }

    private void OnAction1Performed(InputAction.CallbackContext context)
    {
        targetID = 0;
        DetectNear();
    }
    private void OnAction2Performed(InputAction.CallbackContext context)
    {
        targetID = 1;
        DetectNear();
    }
    private void OnAction3Performed(InputAction.CallbackContext context)
    {
        targetID = 2;
        DetectNear();
    }
    private void OnAction4Performed(InputAction.CallbackContext context)
    {
        targetID = 3;
        DetectNear();
    }

    void DetectNear()
    {
        // 現状ソース全選択
        currentHCSs = FindObjectsByType<HierarchyConstraintSource>(FindObjectsSortMode.None);
        // 装着している奴除外
        currentHCSs = CheckEquip();
        // 装着している奴以外のリストから一番近いやつ
        currentHCS = GetNearestTarget(HCTs[targetID].pairRoot.target, currentHCSs);
        // 換装
        ChangeSource();
    }

    // 装着してる奴除外リスト
    HierarchyConstraintSource[] CheckEquip()
    {

        bool constraind = false;
        List<HierarchyConstraintSource> temp = new List<HierarchyConstraintSource>();
        foreach (HierarchyConstraintSource hcs in currentHCSs)
        {
            foreach (HierarchyConstraintTarget hct in HCTs)
            {
                if (hct.pairRoot.source == hcs.source)
                {
                    constraind = true;
                    continue;
                }
            }
            if (!constraind) temp.Add(hcs);
            constraind = false;
        }

        return temp.ToArray();
    }


    //ターゲットから一番近いソースを選ぶ
    public HierarchyConstraintSource GetNearestTarget(Transform target, HierarchyConstraintSource[] sources)
    {
        HierarchyConstraintSource hcs = new HierarchyConstraintSource();
        float minDistanceSqr = Mathf.Infinity; // 最小距離を無限大で初期化
        minDistanceSqr = range;
        Vector3 currentPosition = target.position;

        int i = 0;
        int nearestSourceID = 0;
        foreach (var source in sources)
        {
            var sourcet = source.source.transform;
            // ターゲットからソースへのベクトルを計算
            Vector3 directionToTarget = sourcet.position - currentPosition;

            // 距離の2乗を取得（ルート計算を省くため高速）
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < minDistanceSqr)
            {
                minDistanceSqr = dSqrToTarget;
                nearestSourceID = i;
                hcs = source;
            }
            i++;
        }

        return hcs;
    }


    [ContextMenu("change")]
    public void ChangeSource()
    {
        // ソースが見つからなかったら装着しているソースを外す
        if (currentHCS==null) {
            Transform temp = HCTs[targetID].pairRoot.source.parent;
            HCTs[targetID].pairRoot.source = null;
            HCTs[targetID].RebuildPairs();
            temp.transform.position += new Vector3(range,0f,range);
        }

        HCTs[targetID].hcs = currentHCS;
        HCTs[targetID].pairRoot.source = currentHCS.source;
        HCTs[targetID].offsetRoot = currentHCS.offsetRoot;
        HCTs[targetID].offsetRootRotation = currentHCS.offsetRootRotation;
        HCTs[targetID].RebuildPairs();

        OffsetTop();
        
    }

    void OffsetTop()
    {
        float a = 0f;
        float b = 0f;
        if (HCTs[2].hcs != null) a = HCTs[2].hcs.offsetTop;
        if (HCTs[3].hcs != null) b = HCTs[3].hcs.offsetTop;
        var offsetTop = Mathf.Max(a, b);
        topTransform.localPosition = new Vector3(0, offsetTop, 0);
    }






#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ChangeSource();
    }
#endif
}

