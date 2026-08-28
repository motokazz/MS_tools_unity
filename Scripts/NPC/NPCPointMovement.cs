using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 徘徊
/// </summary>

public class NPCPointMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [Header("移動範囲")]
    [SerializeField] float wanderRadius = 10f; // 移動範囲
    [SerializeField]
    Transform[] points; 
    [Header("次の目的地までの待機時間")]
    [SerializeField] float waitTime = 2f; // 次の目的地へ移動するまでの待機時間
    [Header("速度（ランダム）")]
    [SerializeField] Vector2 speedMinMax = new Vector2(1f,5f);
    [Header("加速度")]
    [SerializeField] float acceleration=1f;
    [Header("旋回速度")]
    [SerializeField] float angulerSpeed=360f;


    //
    int count = 0;
    private float timer;

    void Start()
    {
        Init();
    }

    void Update()
    {
        Timer();
    }

    public void Init()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    public void Timer()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
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
        Vector3 randomDirection = Vector3.zero;
        //目的地設定
        if (points.Length > 0)
        {
            randomDirection = points[count].position;
        }
        count++;
        if (points.Length <= count)
        {
            count = 0;
        }

        //歩き
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.acceleration = acceleration;
            agent.speed = Random.Range(speedMinMax.x,speedMinMax.y);
            agent.angularSpeed = angulerSpeed;
            agent.SetDestination(hit.position);

        }
    }
}