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
    [SerializeField] private float expToNext = 100f; // ���� �������� �ʿ� EXP
    public int Level => level;
    public float Exp => exp;

    [Header("Events")]
    public UnityEvent<float, float> onManaChanged;   // (current, normalized01)
    public UnityEvent<float, float> onExpChanged;    // (current, normalized01)
    public UnityEvent<int> onLeveledUp;              // (newLevel)

    // --- �̺�Ʈ �˸�/����ȭ ---
    public float Mana01 => Mathf.Clamp01(maxMana <= 0f ? 0f : mana / maxMana);
    public float Exp01 => Mathf.Clamp01(expToNext <= 0f ? 1f : exp / expToNext);

    private void NotifyMana() => onManaChanged?.Invoke(mana, Mana01);
    private void NotifyExp() => onExpChanged?.Invoke(exp, Exp01);

    protected override void Awake()
    {
        base.Awake(); // BaseCondition: HP �ʱ� �˸� ȣ��
        // �ʱ� ���µ� UI�� ��� �ݿ�
        NotifyMana();
        NotifyExp();
    }


    // --- �ܺο��� ���� ���� API ---

    // ���� �Ҹ�(��ų ��� ��)
    public bool UseMana(float amount)
    {
        if (amount <= 0) return true;
        if (mana < amount) return false;

        mana -= amount;
        NotifyMana();
        return true;
    }

    // ���� ȸ��(���� ��)
    public void RestoreMana(float amount)
    {
        if (amount <= 0) return;
        mana = Mathf.Min(maxMana, mana + amount);
        NotifyMana();
    }

    // ����ġ ȹ��
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

    // === BaseCondition(HP) ���� BaseCondition�� ������ IValueChangable.ValueChanged(int)�� �״�� ��� ===
    // ������: ValueChanged(-amount), ��: ValueChanged(+amount)

    // --- ���� ���� ---

    private void LevelUp()
    {
        level++;
        onLeveledUp?.Invoke(level);

        // ������ ���ʽ�
        maxMana += 5f;
        mana = maxMana;

        maxHealth += 10f;
        health = maxHealth;
        // ���� ���� �ʿ�ġ(���� ����)
        expToNext = Mathf.Round(expToNext * 1.15f);

        // ������ �� ü��/���� ���� �˸�
        NotifyMana();
        NotifyExp();
        NotifyHealth(); // BaseCondition�� HP �˸�(�ʱ�ȭ/���� ���� ȣ��� ����)

    }

    protected override void Die()
    {
        base.Die(); // onDied �̺�Ʈ ȣ��(���ӿ��� ó�� ���� �����ʿ���)
        // �÷��̾� ���� �߰� ó�� �ʿ��ϸ� ���⿡
    }

    // ===== IDamageable =====
    public void TakeDamage(float damage)
    {
        AddHealth(-Mathf.Abs(damage));
    }

}
