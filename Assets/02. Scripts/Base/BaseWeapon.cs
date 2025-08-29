using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] protected Transform aimRoot; // ĳ�� ȸ�� ����(������ this.transform)
    [SerializeField] protected bool yawOnly = true;   // ����(Yaw)�� ������
    [SerializeField] protected float aimSpeedDegPerSec = 360f; // ȸ�� �ӵ�(��/��)

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

    protected int currentLevel = 1;          // ���Կ��� �����ִ� ���� ����
    protected float runtimeDamage = 0f;      // ���� �ǻ�� ���ݷ�

    protected virtual void Awake()
    {
        if (aimRoot == null) aimRoot = transform;  // �⺻�� �ڱ� �ڽ�
     // TrimFirePointsByData();
    }

    // ���׷��̵�/Ƽ�� ��ȯ �� ȣ��
    public void SetData(WeaponData newData)
    {
        data = newData;
        lastFireTime = -999f;
        //TrimFirePointsByData();
        RecalculateRuntimeDamage();  // �߰�
    }

    // ����/�������� �����ϴ� ���� �޼��� (�Ŵ����� ȣ��)
    public void ApplyLevel(int level, float damagePerLevel)
    {
        currentLevel = Mathf.Max(1, level);
        RecalculateRuntimeDamage(damagePerLevel);
    }
    // ���� ��� �Լ� (������ ���� ������ SO�� �⺻�� ���)
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

        // ���� �ٶ󺸵��� ȸ��
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
        if (yawOnly) dir.y = 0f;                    // ���� ������
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
