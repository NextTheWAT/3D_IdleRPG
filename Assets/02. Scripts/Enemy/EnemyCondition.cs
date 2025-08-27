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

    // �� �������� ü�� ����: '�޴� ���ظ� 1/m��'�� �ٿ��� ü�� ���� ȿ���� ��
    [Header("Stage Toughness")]
    [Tooltip("�������� ������ ���� ������ �޴� ���ظ� 1/m ��� ���ҽ�ŵ�ϴ�. 1�̸� ���� ����.")]
    [SerializeField] private float toughnessMultiplier = 1f;

    [Tooltip("������ HP(+X/��������)�� ������ ȯ���� �� ������ �Ǵ� HP(������ �⺻ ü�� �ٻ簪).")]
    [SerializeField] private float addToMultBaseline = 100f;

    private bool isDead = false;

    protected override void Awake()
    {
        base.Awake();

        // ���۷��� �ڵ� �Ҵ�(�ν����� ������ ���)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();

        toughnessMultiplier = 1f; // �ʱ� ���� ����
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        // �� �������� ���� ����: m�� ��ŷ => �޴� ���ظ� 1/m�� ����
        float scaled = Mathf.Abs(damage) / Mathf.Max(0.01f, toughnessMultiplier);
        AddHealth(-scaled);
        // BaseCondition �ʿ��� 0 ���� �� Die() ȣ�����ִ� �帧�̶�� ���⼭ �� �� �� ����
    }

    // ===== Death =====
    protected override void Die()
    {
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
            toughnessMultiplier = 1f;
            return;
        }

        int n = mod.stage - 1;

        // 1) ������ HP ����: (hpMultPerStage)^n
        float m = 1f;
        if (mod.hpMultPerStage > 0f && !Mathf.Approximately(mod.hpMultPerStage, 1f))
            m *= Mathf.Pow(mod.hpMultPerStage, n);

        // 2) ������ HP ����: +hpAddPerStage*n  ��  ������ �ٻ�: 1 + (������ / ����ü��)
        //    ����ü���� addToMultBaseline(�ν����Ϳ��� ������ �⺻ ü�� �ٻ簪 �Է�)
        if (!Mathf.Approximately(mod.hpAddPerStage, 0f) && addToMultBaseline > 0f)
            m += (mod.hpAddPerStage * n) / addToMultBaseline;

        toughnessMultiplier = Mathf.Max(0.01f, m);
    }
}
