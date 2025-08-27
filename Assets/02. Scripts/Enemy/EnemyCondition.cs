using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // 사망 이펙트
    [SerializeField] private float deathVfxLifetime = 2f; // 파티클에 Destroy 설정이 없을 때만 사용

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // 이동 담당

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // 사망 시 플레이어에게 줄 골드 양

    // ★ 스테이지 체력 보정: '받는 피해를 1/m배'로 줄여서 체력 증가 효과를 냄
    [Header("Stage Toughness")]
    [Tooltip("스테이지 보정에 의해 실제로 받는 피해를 1/m 배로 감소시킵니다. 1이면 보정 없음.")]
    [SerializeField] private float toughnessMultiplier = 1f;

    [Tooltip("가산형 HP(+X/스테이지)를 배율로 환산할 때 기준이 되는 HP(프리팹 기본 체력 근사값).")]
    [SerializeField] private float addToMultBaseline = 100f;

    private bool isDead = false;

    protected override void Awake()
    {
        base.Awake();

        // 레퍼런스 자동 할당(인스펙터 미지정 대비)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();

        toughnessMultiplier = 1f; // 초기 보정 없음
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        // ★ 스테이지 보정 적용: m배 탱킹 => 받는 피해를 1/m로 감소
        float scaled = Mathf.Abs(damage) / Mathf.Max(0.01f, toughnessMultiplier);
        AddHealth(-scaled);
        // BaseCondition 쪽에서 0 이하 시 Die() 호출해주는 흐름이라면 여기서 더 할 일 없음
    }

    // ===== Death =====
    protected override void Die()
    {
        if (isDead) return;   // 재진입 가드
        isDead = true;

        // 이동/히트박스 비활성(있을 때)
        if (enemyAI != null) enemyAI.DisableAgent();

        DeathParticle();

        // 보상 지급(Null 안전)
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldValue);

        // 본체 제거
        Destroy(gameObject, 0.1f);
    }

    public void DeathParticle()
    {
        // 사망 VFX (1회)
        if (deathVfxPrefab != null)
        {
            var vfx = Instantiate(deathVfxPrefab, transform.position, transform.rotation);
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps == null || ps.main.stopAction != ParticleSystemStopAction.Destroy)
                Destroy(vfx, deathVfxLifetime);
        }
    }

    // ====== Stage 연동 훅 ======
    // EnemyManager.StageHealthMod 를 SendMessage/직접 호출로 받는 형태
    public void ApplyStageHealth(EnemyManager.StageHealthMod mod)
    {
        // stage 1은 보정 없음(프리팹 기본치)
        if (mod.stage <= 1)
        {
            toughnessMultiplier = 1f;
            return;
        }

        int n = mod.stage - 1;

        // 1) 배율형 HP 증가: (hpMultPerStage)^n
        float m = 1f;
        if (mod.hpMultPerStage > 0f && !Mathf.Approximately(mod.hpMultPerStage, 1f))
            m *= Mathf.Pow(mod.hpMultPerStage, n);

        // 2) 가산형 HP 증가: +hpAddPerStage*n  →  배율로 근사: 1 + (가산합 / 기준체력)
        //    기준체력은 addToMultBaseline(인스펙터에서 프리팹 기본 체력 근사값 입력)
        if (!Mathf.Approximately(mod.hpAddPerStage, 0f) && addToMultBaseline > 0f)
            m += (mod.hpAddPerStage * n) / addToMultBaseline;

        toughnessMultiplier = Mathf.Max(0.01f, m);
    }
}
