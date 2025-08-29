using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] protected Transform aimRoot; // 캐논 회전 기준(없으면 this.transform)
    [SerializeField] protected bool yawOnly = true;   // 수평(Yaw)만 돌릴지
    [SerializeField] protected float aimSpeedDegPerSec = 360f; // 회전 속도(도/초)

    [Header("Data")]
    [SerializeField] protected WeaponData data;

    [Header("Firing")]
    [SerializeField] protected Transform[] firePoints;
    protected float lastFireTime = -999f;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask losBlockerLayers;
    [SerializeField] private float scanInterval = 0.2f;
    [SerializeField] private bool autoFire = true;

    protected Transform currentTarget;
    private float lastScanTime = -999f;

    protected int currentLevel = 1;          // 슬롯에서 내려주는 현재 레벨
    protected float runtimeDamage = 0f;      // 계산된 실사용 공격력

    protected virtual void Awake()
    {
        if (aimRoot == null) aimRoot = transform;  // 기본은 자기 자신
     // TrimFirePointsByData();
    }

    // 업그레이드/티어 전환 시 호출
    public void SetData(WeaponData newData)
    {
        data = newData;
        lastFireTime = -999f;
        //TrimFirePointsByData();
        RecalculateRuntimeDamage();  // 추가
    }

    // 레벨/증가량을 적용하는 공개 메서드 (매니저가 호출)
    public void ApplyLevel(int level, float damagePerLevel)
    {
        currentLevel = Mathf.Max(1, level);
        RecalculateRuntimeDamage(damagePerLevel);
    }
    // 내부 계산 함수 (증가량 인자 없으면 SO의 기본값 사용)
    protected void RecalculateRuntimeDamage(float damagePerLevelOverride = -1f)
    {
        if (data == null) { runtimeDamage = 0f; return; }
        float per = (damagePerLevelOverride > 0f) ? damagePerLevelOverride : data.damagePerLevel;
        runtimeDamage = data.damage + (Mathf.Max(1, currentLevel) - 1) * per;
    }


    protected virtual void Update()
    {
        if (Time.time - lastScanTime >= scanInterval)
        {
            lastScanTime = Time.time;
            AcquireTarget();
        }

        // 적을 바라보도록 회전
        AimAtTarget();

        if (autoFire && currentTarget != null)
        {
            TryFire();
        }
    }
    protected void AimAtTarget()
    {
        if (aimRoot == null || currentTarget == null) return;

        Vector3 dir = currentTarget.position - aimRoot.position;
        if (yawOnly) dir.y = 0f;                    // 수평만 돌리기
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        aimRoot.rotation = Quaternion.RotateTowards(
            aimRoot.rotation, targetRot, aimSpeedDegPerSec * Time.deltaTime);
    }


    public bool TryFire()
    {
        if (data == null) return false;
        if (Time.time - lastFireTime < data.fireCooldown) return false;

        if (currentTarget == null) AcquireTarget();
        if (currentTarget == null) return false;

        Vector3 muzzle = GetMuzzlePosition();
        float dist = Vector3.Distance(muzzle, currentTarget.position);
        if (dist > data.range) return false;
        if (!HasLineOfSight(muzzle, currentTarget.position)) return false;

        lastFireTime = Time.time;

        OnFire(currentTarget);
        return true;
    }

    protected void AcquireTarget()
    {
        if (data == null) { currentTarget = null; return; }
        var hits = Physics.OverlapSphere(transform.position, data.range, enemyLayer, QueryTriggerInteraction.Ignore);

        Transform best = null;
        float bestSqr = float.MaxValue;
        Vector3 origin = GetMuzzlePosition();

        foreach (var c in hits)
        {
            Vector3 pos = c.bounds.center;
            if (!HasLineOfSight(origin, pos)) continue;

            float sqr = (pos - origin).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = c.transform; }
        }
        currentTarget = best;
    }

    protected bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.0001f) return true;
        dir /= dist;
        return !Physics.Raycast(from, dir, dist, losBlockerLayers, QueryTriggerInteraction.Ignore);
    }

    protected Vector3 GetMuzzlePosition()
    {
        if (firePoints != null && firePoints.Length > 0 && firePoints[0] != null)
            return firePoints[0].position;
        return transform.position;
    }

    protected abstract void OnFire(Transform target);
}
