using UnityEngine;
using UnityEngine.AI; // NavMesh Agentを使うために必要

public class NPCAction : MonoBehaviour
{
    // 公開変数
    [SerializeField] Animator animator;
    [SerializeField] Transform target; // 追跡するターゲット（プレイヤーなど）
    [SerializeField] float attackRange = 2f; // 攻撃できる距離
    [SerializeField] float attackInterval = 2f; // 攻撃の間隔
    [SerializeField] float attackPower = 0.1f;

    // 非公開変数
    float attackBuffer = 0f;

    private NavMeshAgent agent;
    private float timeSinceLastAttack = 0f;

    void Start()
    {
        // コンポーネントを取得
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // ターゲットが設定されていなければ何もしない
        if (target == null)
        {
            return;
        }

        // ターゲットとの距離を計算
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 目的地への移動処理
        if (distanceToTarget > attackRange)
        {
            // 攻撃範囲外なら追跡
            agent.SetDestination(target.position);
        }
        else
        {
            // 攻撃範囲内なら停止
            agent.SetDestination(transform.position);

            // 攻撃処理
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= attackInterval)
            {
                Attack();
                timeSinceLastAttack = 0f;

            }
        }

    }
    void Attack()
    {
        animator.SetTrigger("Attack");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            target = other.transform;
        }
    }
}
