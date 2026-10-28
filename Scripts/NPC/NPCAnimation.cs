using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// NavMeshAgentから撮れる情報をAnimatorのパラメーターに結びつける
/// </summary>

public class NPCAnimation : MonoBehaviour
{
    [Header("アニメーターのあるオブジェクト（未設定OK）")]
    [SerializeField] private GameObject animatorParent;
    [SerializeField] private NavMeshAgent agent;
    [Header("パラメーター名定義")]
    [SerializeField] string speed = "Speed";
    [SerializeField] string angulerSpeed = "AngulerSpeed";
    [SerializeField] float smoothTransition = 0.1f;
    Animator animator;


    void OnEnable()
    {

        Setup();
    }

    private void OnDisable()
    {
        //animatorがあったら初期化する
        if (animator != null) { animator.Rebind(); }
        //NavMeshAgentがあったらストップする
        if (agent != null) { agent.isStopped=true; }
    }

    void Update()
    {
        // Animator側に渡す際、0.1秒かけて滑らかに値を変化させる例
        animator.SetFloat(speed, agent.velocity.magnitude, smoothTransition , Time.deltaTime);
        animator.SetFloat( angulerSpeed , agent.velocity.x, smoothTransition , Time.deltaTime);
    }

    void Setup()
    {
        if (agent == null) { FindAnyObjectByType(typeof(NavMeshAgent)); }
        //NavMeshAgentがあったら開始する
        if (agent != null) { agent.isStopped = false; }

        GameObject targetObject;

        if (animatorParent != null)
        {
            //親のアニメーターの設定があったら
            targetObject = animatorParent.gameObject;
        }
        else
        {
            //無ければ自分がターゲット
            targetObject = gameObject;
        }

        //ターゲットが無かったら
        if (targetObject != null)
        {
            //アニメーターを探す
            animator = targetObject.GetComponent<Animator>();

            //アニメーターが無ければ子からアニメーターを探す
            if (animator == null)
            {
                animator = targetObject.GetComponentInChildren<Animator>();
            }
        }
        else
        {
            //ターゲット無ければ処理しない
            return;
        }

        //animatorがあったら初期化する
        if (animator != null)
        {
            animator.Rebind();
        }
    }
}