using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ĳ����/������Ʈ�� ü��(Health)�� �����ϴ� �߻� Ŭ����.
/// �⺻���� ü�� ����, �̺�Ʈ �˸�, ��� ó�� ���� �����Ѵ�.
/// IValueChangable �������̽��� �����ؼ�, �ܺο��� int �� ���� ��û�� ���ϵ� ������� ó���� �� �ְ� �Ѵ�.
/// </summary>
public abstract class BaseCondition : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float health = 100f;      // ���� ü��
    [SerializeField] protected float minHealth = 0f;     // �ּ� ü�� (���� 0)
    [SerializeField] protected float maxHealth = 100f;   // �ִ� ü��

    [Header("VFX (optional)")]
    [SerializeField] private GameObject deathVfxPrefab;   // ��� ����Ʈ
    [SerializeField] private float deathVfxLifetime = 2f; // ��ƼŬ�� Destroy ������ ���� ���� ���

    private bool isDead = false;

    public float GetHealth() => health;
    public float GetMaxHealth() => maxHealth;

    [Header("Events")]
    // ü�� ��ȭ �� ȣ��Ǵ� �̺�Ʈ: (���� ü�� ��, ����ȭ�� ü�� 0~1)
    public UnityEvent<float, float> onHealthChanged;

    /// <summary>
    /// ���� ü�� �� (�б� ����)
    /// </summary>
    public float Health => health;

    /// <summary>
    /// ���� ü���� [0~1] ������ ����ȭ�� ��
    /// </summary>
    public float Health01 => Mathf.Approximately(maxHealth - minHealth, 0f)
        ? 0f
        : Mathf.Clamp01((health - minHealth) / (maxHealth - minHealth));

    protected virtual void Awake()
    {
        // �ʱ�ȭ �� ü�� ���¸� UI �� �����ʿ� �˸�
        NotifyHealth();
    }

    /// <summary>
    /// ü�� ���� �̺�Ʈ�� �˸�
    /// </summary>
    protected void NotifyHealth()
    {
        onHealthChanged?.Invoke(health, Health01);
    }

    /// <summary>
    /// ü���� ���� (min~max ���� Ŭ����)
    /// </summary>
    public virtual void SetHealth(float value)
    {
        health = Mathf.Clamp(value, minHealth, maxHealth);
        NotifyHealth();

        // ü���� �ּҰ�(=���� ����)�� �����ϸ� Die() ȣ��
        if (Mathf.Approximately(health, minHealth)) Die();
    }

    /// <summary>
    /// ü���� ������Ŵ (���=ȸ��, ����=����)
    /// </summary>
    public virtual void AddHealth(float delta)
    {
        SetHealth(health + delta);
    }

    /// <summary>
    /// ��� ó��: onDied �̺�Ʈ ȣ��
    /// �Ļ� Ŭ�������� �������̵��Ͽ� �߰� ���� ���� ����
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;   // ������ ����
        isDead = true;

        SoundManager.Instance.PlaySfx(SoundManager.SfxId.Explosion);
        DeathParticle();
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
