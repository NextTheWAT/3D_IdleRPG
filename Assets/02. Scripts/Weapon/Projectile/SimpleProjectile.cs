using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("VFX/SFX (optional)")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float fallbackVfxLifetime = 2f;

    [Header("Collision")]
    [Tooltip("이 레이어에 속한 오브젝트와의 충돌은 무시합니다.")]
    [SerializeField] private LayerMask ignoreLayers;

    [Header("Trail Handling")]
    [Tooltip("히트 시 TrailRenderer를 본체에서 분리한 뒤 잠시 후 제거합니다.")]
    [SerializeField] private bool detachTrailOnHit = true;
    [SerializeField] private float trailLifetimeAfterHit = 0.3f; // 트레일 잔상 유지 시간

    private float damage;
    private float speed;
    private float maxDistance;
    private Vector3 startPos;
    private bool hasHit = false;

    public void Init(float damage, float speed, float maxDistance)
    {
        this.damage = damage;
        this.speed = speed;
        this.maxDistance = maxDistance;
        startPos = transform.position;
        hasHit = false;
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        if ((transform.position - startPos).sqrMagnitude >= maxDistance * maxDistance)
        {
            Explode(transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldIgnore(other.gameObject)) return;
        HandleHit(other, other.ClosestPoint(transform.position));
    }


    private bool ShouldIgnore(GameObject obj)
    {
        return ((1 << obj.layer) & ignoreLayers) != 0;
    }

    private void HandleHit(Collider other, Vector3 hitPos)
    {
        if (hasHit) return;
        hasHit = true;

        if (other != null && other.CompareTag("Enemy"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
        }

        SpawnImpactVfx(hitPos, -transform.forward);
        DetachAndCleanupTrails();   // ★ 트레일 분리/정리 추가
        Destroy(gameObject);
    }

    private void Explode(Vector3 pos)
    {
        if (hasHit) return;
        hasHit = true;

        SpawnImpactVfx(pos, -transform.forward);
        DetachAndCleanupTrails();   // 트레일 분리/정리 추가
        Destroy(gameObject);
    }

    private void SpawnImpactVfx(Vector3 position, Vector3 normal)
    {
        if (impactVfxPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(
            normal.sqrMagnitude > 0.0001f ? normal.normalized : -transform.forward,
            Vector3.up);

        var vfx = Instantiate(impactVfxPrefab, position, rot);
        var ps = vfx.GetComponent<ParticleSystem>();
        if (ps == null || ps.main.stopAction != ParticleSystemStopAction.Destroy)
        {
            Destroy(vfx, fallbackVfxLifetime);
        }
    }

    // TrailRenderer 안전 처리
    private void DetachAndCleanupTrails()
    {
        if (!detachTrailOnHit) return;

        // 자신과 자식들에서 TrailRenderer 모두 찾기
        var trails = GetComponentsInChildren<TrailRenderer>(true);
        foreach (var tr in trails)
        {
            if (tr == null) continue;

            // 본체에서 분리해서 씬에 남도록
            tr.transform.SetParent(null, worldPositionStays: true);

            // 더 이상 새로운 꼬리 생성 안 하게
            tr.emitting = false;

            // 잔상이 사라질 시간을 조금 준 뒤 제거
            Destroy(tr.gameObject, Mathf.Max(0.01f, trailLifetimeAfterHit));
        }
    }
}
