using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : Singleton<EnemyManager>
{
    [Header("Enemy Prefabs (랜덤 선택)")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    [Header("Spawn Mode (둘 중 하나 사용)")]
    [SerializeField] private BoxCollider navmeshArea;   // 이 박스 범위 안에서만 스폰(권장)
    [SerializeField] private Transform[] spawnAnchors;  // 또는 앵커들 중 하나 주변 반경에서 스폰
    [SerializeField] private float anchorRadius = 10f;  // 앵커 기준 반경

    [Header("Spawn Rules")]
    [SerializeField] private int maxAlive = 10;         // 동시 존재 최대 수
    [SerializeField] private float spawnInterval = 2f;  // 스폰 간격(초)
    [SerializeField] private int spawnPerTick = 1;      // 틱당 소환 수

    [Header("NavMesh Settings")]
    [SerializeField] private string[] allowedAreaNames = { "Walkable" }; // 허용 NavMesh Area
    [SerializeField] private float sampleMaxDistance = 2f; // 랜덤 점 근처에서 NavMesh 샘플링 탐색거리

    [Header("Misc")]
    [SerializeField] private bool randomYRotation = true;

    private readonly List<GameObject> alive = new();
    private int areaMask;

    private void Awake()
    {
        BuildAreaMask();

        // BoxCollider를 스폰 영역으로 쓰는 경우, isTrigger 권장
        if (navmeshArea != null) navmeshArea.isTrigger = true;
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
                    SpawnOne(spawnPos);
                }
            }

            yield return wait;
        }
    }

    private void SpawnOne(Vector3 position)
    {
        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Quaternion rot = randomYRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : prefab.transform.rotation;

        var go = Instantiate(prefab, position, rot);
        alive.Add(go);
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
        // 여러 번 시도해서 NavMesh 위 좌표를 찾는다
        const int maxTries = 30;

        for (int i = 0; i < maxTries; i++)
        {
            Vector3 candidate = (navmeshArea != null)
                ? RandomPointInBox(navmeshArea.bounds)
                : RandomPointNearAnchor();

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
        float y = b.center.y; // 높이는 샘플링에서 보정될 것
        float z = Random.Range(b.min.z, b.max.z);
        return new Vector3(x, y, z);
    }

    private Vector3 RandomPointNearAnchor()
    {
        Transform anchor = (spawnAnchors != null && spawnAnchors.Length > 0)
            ? spawnAnchors[Random.Range(0, spawnAnchors.Length)]
            : transform;

        Vector2 r = Random.insideUnitCircle * anchorRadius;
        return new Vector3(anchor.position.x + r.x, anchor.position.y, anchor.position.z + r.y);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 영역 시각화
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        if (navmeshArea != null)
        {
            var b = navmeshArea.bounds;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else if (spawnAnchors != null)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
            foreach (var t in spawnAnchors)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.position, anchorRadius);
            }
        }
    }
#endif
}
