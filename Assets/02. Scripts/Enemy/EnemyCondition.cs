using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // ��� ����Ʈ
    [SerializeField] private float deathVfxLifetime = 2f; // ��ƼŬ�� Destroy ������ ���� ���� ���

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // �̵� ���

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // ��� �� �÷��̾�� �� ��� ��

    [Header("Stage Scaling (Absolute)")]
    [SerializeField] private float baseMaxHealthCache = -1f; // ���� ���� ü��

    private bool isDead = false;

    protected override void Awake()
    {
        base.Awake();

        // ���۷��� �ڵ� �Ҵ�(�ν����� ������ ���)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        AddHealth(-damage);
        // BaseCondition �ʿ��� 0 ���� �� Die() ȣ�����ִ� �帧�̶�� ���⼭ �� �� �� ����
    }

    // ===== Death =====
    protected override void Die()
    {
        base.Die();
        if (isDead) return;   // ������ ����
        isDead = true;

        // �̵�/��Ʈ�ڽ� ��Ȱ��(���� ��)
        if (enemyAI != null) enemyAI.DisableAgent();

        DeathParticle();

        // ���� ����(Null ����)
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldValue);

        // ��ü ����
        Destroy(gameObject, 0.1f);
    }

    public void DeathParticle()
    {
        // ��� VFX (1ȸ)
        if (deathVfxPrefab != null)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, transform.rotation);
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps == null || ps.main.stopAction != ParticleSystemStopAction.Destroy)
                Destroy(vfx, deathVfxLifetime);
        }
    }

    // ====== Stage ���� �� ======
    // EnemyManager.StageHealthMod �� SendMessage/���� ȣ��� �޴� ����
    public void ApplyStageHealth(EnemyManager.StageHealthMod mod)
    {
        // stage 1�� ���� ����(������ �⺻ġ)
        if (mod.stage <= 1)
        {
            SetMaxHealthAbsolute(baseMaxHealthCache, keepRatio: true);
            return;
        }

        int n = mod.stage - 1;
        float newMax = baseMaxHealthCache + (mod.hpAddPerStage * n);
        SetMaxHealthAbsolute(newMax, keepRatio: true);
    }
    public void SetMaxHealthAbsolute(float newMax, bool keepRatio = true)
    {
        newMax = Mathf.Max(1f, newMax);
        float ratio = Mathf.Approximately(maxHealth, 0f) ? 1f : (health / maxHealth);
        maxHealth = newMax;
        health = keepRatio ? newMax * ratio : Mathf.Min(health, newMax);

    }

}
