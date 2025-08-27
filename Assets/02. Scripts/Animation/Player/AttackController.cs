using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private int manaCost = 20;

    private readonly int spinHash = Animator.StringToHash("Spin_Attack");
    private readonly int gunHash = Animator.StringToHash("Gun_Attack");

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 스핀 어택 실행 (Trigger)
    /// </summary>
    public void PlaySpinAttack()
    {
        if(GameManager.Instance.playerCondition.Mana < manaCost) return;
        animator.SetTrigger(spinHash);
        GameManager.Instance.playerCondition.UseMana(manaCost);
    }

    /// <summary>
    /// 총 공격 애니메이션 상태를 제어 (Bool)
    /// </summary>
    public void SetGunAttack(bool active)
    {
        animator.SetBool(gunHash, active);
    }
}
