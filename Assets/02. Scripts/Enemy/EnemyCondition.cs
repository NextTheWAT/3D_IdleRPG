using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // ��� ����Ʈ
    [SerializeField] private float deathVfxLifetime = 2f; // ��ƼŬ�� Destroy ������ ���� ���� ���

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // �̵� ���
    [SerializeField] private EnemyAttack enemyAttack;     // ����(��Ʈ�ڽ�) ���

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // ��� �� �÷��̾�� �� ��� ��

    private bool isDead = false;

    protected override void Awake()
    {
        base.Awake();

        // ���۷��� �ڵ� �Ҵ�(�ν����� ������ ���)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        if (enemyAttack == null) enemyAttack = GetComponentInChildren<EnemyAttack>();
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        // BaseCondition�� ���������� ü�� ����/��� Ʈ���Ÿ� ó���Ѵٰ� ����
        AddHealth(-Mathf.Abs(damage));
        // �ʿ� ��: ü�� 0 ���� üũ�� BaseCondition�� ���ٸ� ���⼭ Die() ȣ�� ������ �߰�
        // if (CurrentHealth <= 0f) Die();
    }

    // ===== Death =====
    protected override void Die()
    {
        CurrencyManager.Instance.AddGold(goldValue); 

        if (isDead) return;   // ������ ����
        isDead = true;

        if (enemyAI != null) enemyAI.DisableAgent();

        DeathParticle();

        // ��ü ����
        Destroy(gameObject);
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
}
