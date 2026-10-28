using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 滑らかな徘徊処理（停止時のジャンプ軽減版）
/// </summary>
public class SmoothNPCWander : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    [Header("移動範囲")]
    [SerializeField] float wanderRadius = 10f;

    [Header("次の目的地までの待機時間")]
    [SerializeField] float waitTime = 2f;

    [Header("速度（ランダム）")]
    [SerializeField] Vector2 speedMinMax = new Vector2(1f, 5f);

    [Header("加速度")]
    [SerializeField] float acceleration = 8f; // ※少し高めにすると自然な発進になります

    [Header("旋回速度")]
    [SerializeField] float angularSpeed = 360f;

    [Header("停止距離")]
    [SerializeField] float stoppingDistance = 0.5f;

    private float timer;
    private bool isWaiting = false;

    void Start()
    {
        Init();
    }

    void Update()
    {
        HandleWander();
    }

    public void Init()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // 滑らかに停止させるための設定
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;

        SetNewDestination();
    }

    private void HandleWander()
    {
        // 経路計算中なら何もしない
        if (agent.pathPending) return;

        // 目的地に到達したかの判定
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                // 到着した瞬間に完全に停止させる（ジャンプ防止）
                isWaiting = true;
                agent.isStopped = true;
                agent.ResetPath(); // 現在のパスを破棄して微振動を防ぐ
            }

            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                SetNewDestination();
                timer = 0;
            }
        }
    }

    void SetNewDestination()
    {
        isWaiting = false;

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        // NavMesh上の有効なポイントを見つける
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            // エージェントの移動を再開
            agent.isStopped = false;
            agent.acceleration = acceleration;
            agent.speed = Random.Range(speedMinMax.x, speedMinMax.y);
            agent.angularSpeed = angularSpeed;

            agent.SetDestination(hit.position);
        }
        else
        {
            // 失敗した場合は待機状態にして、次回すぐに再検索する
            isWaiting = true;
            timer = waitTime;
        }
    }
}