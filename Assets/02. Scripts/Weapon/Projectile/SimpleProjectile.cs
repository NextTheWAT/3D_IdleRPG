using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("VFX/SFX (optional)")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float fallbackVfxLifetime = 2f;

    [Header("Collision")]
    [Tooltip("이 레이어에 속한 오브젝트와의 충돌은 무시합니다.")]
    [SerializeField] private LayerMask ignoreLayers; // 무시할 레이어들

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
        if (ShouldIgnore(other.gameObject)) return; // 레이어 무시
        HandleHit(other, other.ClosestPoint(transform.position));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ShouldIgnore(collision.gameObject)) return; // 레이어 무시

        Vector3 hitPos = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : collision.collider.ClosestPoint(transform.position);

        HandleHit(collision.collider, hitPos);
    }

    private bool ShouldIgnore(GameObject obj)
    {
        // 비트마스크 비교: obj.layer가 ignoreLayers에 포함돼 있으면 무시
        return ((1 << obj.layer) & ignoreLayers) != 0;
    }

    private void HandleHit(Collider other, Vector3 hitPos)
    {
        if (hasHit) return;
        hasHit = true;

        // Enemy 태그면 데미지 적용
        if (other != null && other.CompareTag("Enemy"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
        }

        SpawnImpactVfx(hitPos, -transform.forward);
        Destroy(gameObject);
    }

    private void Explode(Vector3 pos)
    {
        if (hasHit) return;
        hasHit = true;

        SpawnImpactVfx(pos, -transform.forward);
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
}
