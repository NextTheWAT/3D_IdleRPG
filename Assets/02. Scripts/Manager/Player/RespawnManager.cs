using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Header("Target (optional, ����θ� �ڵ� Ž��)")]
    [SerializeField] private CarAI car;           // �÷��̾� �ڵ��� CarAI
    [SerializeField] private Transform carTf;     // ���� Transform(������ �� car.transform ���)

    [Header("Spawn Options")]
    [SerializeField] private bool snapToNavMesh = true;
    [SerializeField] private float spawnYOffset = 0.2f;

    void Awake() => ResolveRefs();
    void OnEnable() => ResolveRefs();

    private void ResolveRefs()
    {
        // 1) �ν����� ���� �켱
        if (!car)
        {
            // 2) PlayerManager�� ������ �켱 ���
            if (PlayerManager.Instance && PlayerManager.Instance.carAI)
                car = PlayerManager.Instance.carAI;
            // 3) �׷��� ������ ������ �˻�
            if (!car) car = FindObjectOfType<CarAI>();
        }

        if (!carTf)
        {
            if (PlayerManager.Instance && PlayerManager.Instance.carTransform)
                carTf = PlayerManager.Instance.carTransform;
            if (!carTf && car) carTf = car.transform;
        }
    }

    // UI Button OnClick�� ����
    public void RespawnToNearestTrackNode()
    {
        ResolveRefs();

        if (!car)
        {
            Debug.LogWarning("[RespawnManager] CarAI�� ã�� ���߽��ϴ�.");
            return;
        }
        if (car.trackNodes == null || car.trackNodes.Count == 0)
        {
            Debug.LogWarning("[RespawnManager] trackNodes�� ��� �ֽ��ϴ�. CarAI�� ��带 ����ϼ���.");
            return;
        }

        // ���� ��ġ(�����ϸ� �� �պκ� ����)�κ��� ���� ����� ��� ã��
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
