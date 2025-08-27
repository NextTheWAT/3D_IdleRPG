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
    /// ���� ���� ���� (Trigger)
    /// </summary>
    public void PlaySpinAttack()
    {
        if(GameManager.Instance.playerCondition.Mana < manaCost) return;
        animator.SetTrigger(spinHash);
        GameManager.Instance.playerCondition.UseMana(manaCost);
    }

    /// <summary>
    /// �� ���� �ִϸ��̼� ���¸� ���� (Bool)
    /// </summary>
    public void SetGunAttack(bool active)
    {
        animator.SetBool(gunHash, active);
    }
}
