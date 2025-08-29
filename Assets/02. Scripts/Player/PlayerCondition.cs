using UnityEngine;
using UnityEngine.Events;

public class PlayerCondition : BaseCondition, IDamageable
{
    [Header("Mana")]
    [SerializeField] private float mana = 50f;
    [SerializeField] private float maxMana = 50f;

    public float Mana => mana;
    public float MaxMana => maxMana;

    [Header("Experience / Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private float exp = 0f;
    [SerializeField] private float expToNext = 100f; // 다음 레벨까지 필요 EXP
    public int Level => level;
    public float Exp => exp;

    [Header("Events")]
    public UnityEvent<float, float> onManaChanged;   // (current, normalized01)
    public UnityEvent<float, float> onExpChanged;    // (current, normalized01)
    public UnityEvent<int> onLeveledUp;              // (newLevel)

    // --- 이벤트 알림/정규화 ---
    public float Mana01 => Mathf.Clamp01(maxMana <= 0f ? 0f : mana / maxMana);
    public float Exp01 => Mathf.Clamp01(expToNext <= 0f ? 1f : exp / expToNext);

    private void NotifyMana() => onManaChanged?.Invoke(mana, Mana01);
    private void NotifyExp() => onExpChanged?.Invoke(exp, Exp01);

    protected override void Awake()
    {
        base.Awake(); // BaseCondition: HP 초기 알림 호출
        // 초기 상태도 UI에 즉시 반영
        NotifyMana();
        NotifyExp();
    }


    // --- 외부에서 쓰는 간단 API ---

    // 마나 소모(스킬 사용 등)
    public bool UseMana(float amount)
    {
        if (amount <= 0) return true;
        if (mana < amount) return false;

        mana -= amount;
        NotifyMana();
        return true;
    }

    // 마나 회복(포션 등)
    public void RestoreMana(float amount)
    {
        if (amount <= 0) return;
        mana = Mathf.Min(maxMana, mana + amount);
        NotifyMana();
    }

    // 경험치 획득
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        exp += amount;
        while (exp >= expToNext)
        {
            exp -= expToNext;
            LevelUp();
        }
        NotifyExp();
    }

    // === BaseCondition(HP) 쪽은 BaseCondition이 구현한 IValueChangable.ValueChanged(int)를 그대로 사용 ===
    // 데미지: ValueChanged(-amount), 힐: ValueChanged(+amount)

    // --- 내부 동작 ---

    private void LevelUp()
    {
        level++;
        onLeveledUp?.Invoke(level);

        // 레벨업 보너스
        maxMana += 5f;
        mana = maxMana;

        maxHealth += 10f;
        health = maxHealth;
        // 다음 레벨 필요치(점진 증가)
        expToNext = Mathf.Round(expToNext * 1.15f);

        // 레벨업 시 체력/마나 갱신 알림
        NotifyMana();
        NotifyExp();
        NotifyHealth(); // BaseCondition의 HP 알림(초기화/변경 직후 호출과 동일)

    }

    protected override void Die()
    {
        base.Die(); // onDied 이벤트 호출(게임오버 처리 등은 리스너에서)
        // 플레이어 전용 추가 처리 필요하면 여기에
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        AddHealth(-Mathf.Abs(damage));
    }

}
