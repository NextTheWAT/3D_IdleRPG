using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnSnapshot : MonoBehaviour
{
    public float baseSpeed;
}

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Prefabs (랜덤 선택)")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Area (둘 중 하나 사용)")]
    [SerializeField] private BoxCollider navmeshArea;   // 이 박스 범위 안에서 스폰

    [Header("Spawn Rules")]
    [SerializeField] private int maxAlive = 10;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int spawnPerTick = 1;

    [Header("NavMesh Filter")]
    [SerializeField] private string[] allowedAreaNames = { "Walkable" };
    [SerializeField] private float sampleMaxDistance = 2f;

    [Header("Buff Targets")]
    [Tooltip("새로 스폰되는 적에만 버프 적용")]
    [SerializeField] private bool buffOnlyNewSpawns = true;
    [Tooltip("스테이지가 바뀔 때 이미 살아있는 적에게도 즉시 버프 적용")]
    [SerializeField] private bool buffAliveOnStageChange = false;

    private readonly List<GameObject> alive = new();
    private int areaMask;

    // HP 보정용 페이로드(선택적 훅)
    public struct StageHealthMod
    {
        public int stage;
        public float hpAddPerStage;
        public float hpMultPerStage;
    }

    private void Awake()
    {
        BuildAreaMask();
        if (navmeshArea != null) navmeshArea.isTrigger = true;
    }

    private void OnEnable()
    {
        if (StageManager.IsInitialized)
            StageManager.Instance.OnStageChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        if (StageManager.IsInitialized)
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
    }

    private void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemyManager] enemyPrefabs가 비어 있습니다.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            CompactAliveList();

            int deficit = maxAlive - alive.Count;
            int toSpawn = Mathf.Min(spawnPerTick, Mathf.Max(0, deficit));

            for (int i = 0; i < toSpawn; i++)
            {
                if (TryGetRandomNavmeshPosition(out Vector3 spawnPos))
                {
                    var go = SpawnOne(spawnPos);
                    if (go != null && (!buffOnlyNewSpawns || buffOnlyNewSpawns))
                    {
                        ApplyStageBuff(go); // 새로 스폰된 적 버프
                    }
                }
            }
            yield return wait;
        }
    }
    private GameObject SpawnOne(Vector3 position)
    {
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        var go = Instantiate(prefab, position, rot);
        alive.Add(go);

        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent)
        {
            var snap = go.AddComponent<EnemySpawnSnapshot>();
            snap.baseSpeed = agent.speed; // 기준 속도 저장
        }
        return go;
    }

    private void CompactAliveList()
    {
        for (int i = alive.Count - 1; i >= 0; --i)
        {
            if (alive[i] == null) alive.RemoveAt(i);
        }
    }

    private bool TryGetRandomNavmeshPosition(out Vector3 position)
    {
        const int maxTries = 30;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 candidate = RandomPointInBox(navmeshArea.bounds);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleMaxDistance, areaMask))
            {
                position = hit.position;
                return true;
            }
        }

        position = default;
        return false;
    }

    private Vector3 RandomPointInBox(Bounds b)
    {
        float x = Random.Range(b.min.x, b.max.x);
        float y = b.center.y;
        float z = Random.Range(b.min.z, b.max.z);
        return new Vector3(x, y, z);
    }


    private void BuildAreaMask()
    {
        int mask = 0;
        if (allowedAreaNames != null && allowedAreaNames.Length > 0)
        {
            foreach (var name in allowedAreaNames)
            {
                int idx = NavMesh.GetAreaFromName(name);
                if (idx >= 0) mask |= 1 << idx;
                else Debug.LogWarning($"[EnemyManager] NavMesh Area '{name}' 를 찾지 못했습니다.");
            }
        }
        areaMask = (mask == 0) ? NavMesh.AllAreas : mask;
    }

    // --------- Stage 연동 ---------

    private void HandleStageChanged(int newStage)
    {
        if (!buffAliveOnStageChange) return;

        // 살아있는 적 전체에 즉시 버프 적용
        for (int i = alive.Count - 1; i >= 0; --i)
        {
            if (alive[i] != null) ApplyStageBuff(alive[i]);
        }
    }

    private void ApplyStageBuff(GameObject enemyRoot)
    {
        if (!StageManager.IsInitialized) return;
        int stage = StageManager.Instance.CurrentStage;
        if (stage <= 1) return;

        int n = stage - 1;

        // 2) HP: EnemyCondition이 절대 가산형으로 처리
        var mod = new StageHealthMod
        {
            stage = stage,
            hpAddPerStage = StageManager.Instance.hpAddPerStage,
            hpMultPerStage = 0f // 더이상 사용 안 함
        };
        enemyRoot.SendMessage("ApplyStageHealth", mod, SendMessageOptions.DontRequireReceiver);
    }
}
