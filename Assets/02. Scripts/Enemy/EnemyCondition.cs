using UnityEngine;

public class EnemyCondition : BaseCondition, IDamageable
{
    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // 사망 이펙트
    [SerializeField] private float deathVfxLifetime = 2f; // 파티클에 Destroy 설정이 없을 때만 사용

    [Header("References")]
    [SerializeField] private EnemyAI enemyAI;             // 이동 담당
    [SerializeField] private EnemyAttack enemyAttack;     // 공격(히트박스) 담당

    [Header("Gold Value")]
    [SerializeField] private int goldValue = 10;          // 사망 시 플레이어에게 줄 골드 양

    private bool isDead = false;

    protected override void Awake()
    {
        base.Awake();

        // 레퍼런스 자동 할당(인스펙터 미지정 대비)
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        if (enemyAttack == null) enemyAttack = GetComponentInChildren<EnemyAttack>();
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        // BaseCondition이 내부적으로 체력 관리/사망 트리거를 처리한다고 가정
        AddHealth(-Mathf.Abs(damage));
        // 필요 시: 체력 0 이하 체크가 BaseCondition에 없다면 여기서 Die() 호출 로직을 추가
        // if (CurrentHealth <= 0f) Die();
    }

    // ===== Death =====
    protected override void Die()
    {
        CurrencyManager.Instance.AddGold(goldValue); 

        if (isDead) return;   // 재진입 가드
        isDead = true;

        if (enemyAI != null) enemyAI.DisableAgent();

        DeathParticle();

        // 본체 제거
        Destroy(gameObject);
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
}
