using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 캐릭터/오브젝트의 체력(Health)을 관리하는 추상 클래스.
/// 기본적인 체력 증감, 이벤트 알림, 사망 처리 등을 제공한다.
/// IValueChangable 인터페이스를 구현해서, 외부에서 int 값 증감 요청을 통일된 방식으로 처리할 수 있게 한다.
/// </summary>
public abstract class BaseCondition : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float health = 100f;      // 현재 체력
    [SerializeField] protected float minHealth = 0f;     // 최소 체력 (보통 0)
    [SerializeField] protected float maxHealth = 100f;   // 최대 체력

    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // 사망 이펙트
    [SerializeField] private float deathVfxLifetime = 2f; // 파티클에 Destroy 설정이 없을 때만 사용

    private bool isDead = false;

    public float GetHealth() => health;
    public float GetMaxHealth() => maxHealth;

    [Header("Events")]
    // 체력 변화 시 호출되는 이벤트: (현재 체력 값, 정규화된 체력 0~1)
    public UnityEvent<float, float> onHealthChanged;

    /// <summary>
    /// 현재 체력 값 (읽기 전용)
    /// </summary>
    public float Health => health;

    /// <summary>
    /// 현재 체력을 [0~1] 범위로 정규화한 값
    /// </summary>
    public float Health01 => Mathf.Approximately(maxHealth - minHealth, 0f)
        ? 0f
        : Mathf.Clamp01((health - minHealth) / (maxHealth - minHealth));

    protected virtual void Awake()
    {
        // 초기화 시 체력 상태를 UI 등 리스너에 알림
        NotifyHealth();
    }

    /// <summary>
    /// 체력 변경 이벤트를 알림
    /// </summary>
    protected void NotifyHealth()
    {
        onHealthChanged?.Invoke(health, Health01);
    }

    /// <summary>
    /// 체력을 설정 (min~max 범위 클램핑)
    /// </summary>
    public virtual void SetHealth(float value)
    {
        health = Mathf.Clamp(value, minHealth, maxHealth);
        NotifyHealth();

        // 체력이 최소값(=죽음 상태)에 도달하면 Die() 호출
        if (Mathf.Approximately(health, minHealth)) Die();
    }

    /// <summary>
    /// 체력을 증감시킴 (양수=회복, 음수=피해)
    /// </summary>
    public virtual void AddHealth(float delta)
    {
        SetHealth(health + delta);
    }

    /// <summary>
    /// 사망 처리: onDied 이벤트 호출
    /// 파생 클래스에서 오버라이드하여 추가 동작 구현 가능
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;   // 재진입 가드
        isDead = true;

        SoundManager.Instance.PlaySfx(SoundManager.SfxId.Explosion);
        DeathParticle();
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
