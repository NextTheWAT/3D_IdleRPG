using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private int manaCost = 20;

    private readonly int spinHash = Animator.StringToHash("Spin_Attack");

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 스핀 어택 실행 (Trigger)
    /// </summary>
    public void PlaySpinAttack()
    {
        SoundManager.Instance.PlaySfx(SoundManager.SfxId.SpinAttack);
        if (GameManager.Instance.playerCondition.Mana < manaCost) return;
        animator.SetTrigger(spinHash);
        GameManager.Instance.playerCondition.UseMana(manaCost);
    }
}
