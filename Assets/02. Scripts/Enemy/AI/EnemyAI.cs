using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform player;          // 비우면 태그로 자동 탐색
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Agent Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float angularSpeed = 720f;
    [SerializeField] private float acceleration = 16f;
    [SerializeField] private bool updateRotation = true;

    private NavMeshAgent agent;
    private float lastRepathTime = -999f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ApplyAgentParams();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
            return;
        }

        if (Time.time - lastRepathTime >= repathInterval)
        {
            lastRepathTime = Time.time;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > stopDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

    private void ApplyAgentParams()
    {
        agent.speed = moveSpeed;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stopDistance;
        agent.updateRotation = updateRotation;
    }

    // ===== Public APIs =====
    public void SetPlayer(Transform t)
    {
        player = t;
        if (agent != null && agent.enabled && agent.isOnNavMesh && player != null)
            agent.SetDestination(player.position);
    }

    public void SetMoveParams(float speed, float angular, float accel)
    {
        moveSpeed = speed; angularSpeed = angular; acceleration = accel;
        if (agent != null) ApplyAgentParams();
    }

    /// <summary>사망 시 호출: 에이전트 즉시 정지/비활성화</summary>
    public void DisableAgent()
    {
        if (agent == null) return;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        agent.enabled = false;
    }
}
