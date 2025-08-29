using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Header("Target (optional, 비워두면 자동 탐색)")]
    [SerializeField] private CarAI car;           // 플레이어 자동차 CarAI
    [SerializeField] private Transform carTf;     // 차량 Transform(미지정 시 car.transform 사용)

    [Header("Spawn Options")]
    [SerializeField] private bool snapToNavMesh = true;
    [SerializeField] private float spawnYOffset = 0.2f;

    void Awake() => ResolveRefs();
    void OnEnable() => ResolveRefs();

    private void ResolveRefs()
    {
        // 1) 인스펙터 지정 우선
        if (!car)
        {
            // 2) PlayerManager에 있으면 우선 사용
            if (PlayerManager.Instance && PlayerManager.Instance.carAI)
                car = PlayerManager.Instance.carAI;
            // 3) 그래도 없으면 씬에서 검색
            if (!car) car = FindObjectOfType<CarAI>();
        }

        if (!carTf)
        {
            if (PlayerManager.Instance && PlayerManager.Instance.carTransform)
                carTf = PlayerManager.Instance.carTransform;
            if (!carTf && car) carTf = car.transform;
        }
    }

    // UI Button OnClick에 연결
    public void RespawnToNearestTrackNode()
    {
        ResolveRefs();

        if (!car)
        {
            Debug.LogWarning("[RespawnManager] CarAI를 찾지 못했습니다.");
            return;
        }
        if (car.trackNodes == null || car.trackNodes.Count == 0)
        {
            Debug.LogWarning("[RespawnManager] trackNodes가 비어 있습니다. CarAI에 노드를 등록하세요.");
            return;
        }

        // 현재 위치(가능하면 차 앞부분 기준)로부터 가장 가까운 노드 찾기
        Vector3 from = car.carFront ? car.carFront.position : (carTf ? carTf.position : car.transform.position);

        int nearestIdx = FindNearestNodeIndex(from, car.trackNodes);
        car.TeleportToNode(nearestIdx, alignForward: true, snapToNavMesh: snapToNavMesh, yOffset: spawnYOffset);
    }

    private static int FindNearestNodeIndex(Vector3 from, List<Transform> nodes)
    {
        int bestIdx = -1;
        float best = float.PositiveInfinity;
        Vector3 flatFrom = new Vector3(from.x, 0f, from.z);

        for (int i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (!n) continue;
            Vector3 p = n.position; p.y = 0f;
            float d2 = (p - flatFrom).sqrMagnitude;
            if (d2 < best) { best = d2; bestIdx = i; }
        }
        return Mathf.Max(bestIdx, 0);
    }
}
