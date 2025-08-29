using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // �̵� ���

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // ��� �� �÷��̾�� �� ��� ��

    [Header("Stage Scaling (Absolute)")]
    [SerializeField] private float baseMaxHealthCache = -1f; // ���� ���� ü��


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

        // �̵�/��Ʈ�ڽ� ��Ȱ��(���� ��)
        if (enemyAI != null) enemyAI.DisableAgent();

        PlayerManager.Instance.playerCondition.AddExp(10); // ����ġ ȹ��(����ġ, ���� ���� ����)

        // ���� ����(Null ����)
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldValue);

        // ��ü ����
        Destroy(gameObject, 0.1f);
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
