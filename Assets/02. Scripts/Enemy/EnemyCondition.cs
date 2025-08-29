using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // 이동 담당

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // 사망 시 플레이어에게 줄 골드 양

    [Header("Stage Scaling (Absolute)")]
    [SerializeField] private float baseMaxHealthCache = -1f; // 스폰 기준 체력


    protected override void Awake()
    {
        base.Awake();

        // 레퍼런스 자동 할당(인스펙터 미지정 대비)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        AddHealth(-damage);
        // BaseCondition 쪽에서 0 이하 시 Die() 호출해주는 흐름이라면 여기서 더 할 일 없음
    }

    // ===== Death =====
    protected override void Die()
    {
        base.Die();

        // 이동/히트박스 비활성(있을 때)
        if (enemyAI != null) enemyAI.DisableAgent();

        PlayerManager.Instance.playerCondition.AddExp(10); // 경험치 획득(고정치, 추후 조정 가능)

        // 보상 지급(Null 안전)
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldValue);

        // 본체 제거
        Destroy(gameObject, 0.1f);
    }


    // ====== Stage 연동 훅 ======
    // EnemyManager.StageHealthMod 를 SendMessage/직접 호출로 받는 형태
    public void ApplyStageHealth(EnemyManager.StageHealthMod mod)
    {
        // stage 1은 보정 없음(프리팹 기본치)
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
