using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private readonly int spinHash = Animator.StringToHash("Spin_Attack");
    private readonly int gunHash = Animator.StringToHash("Gun_Attack");

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// ���� ���� ���� (Trigger)
    /// </summary>
    public void PlaySpinAttack()
    {
        animator.SetTrigger(spinHash);
    }

    /// <summary>
    /// �� ���� �ִϸ��̼� ���¸� ���� (Bool)
    /// </summary>
    public void SetGunAttack(bool active)
    {
        animator.SetBool(gunHash, active);
    }
}
