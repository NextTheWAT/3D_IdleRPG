using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [SerializeField] public PlayerCondition playerCondition;
    [SerializeField] private AttackController attackController;


    private void Awake()
    {
        if (!playerCondition) playerCondition = PlayerManager.Instance.playerCondition;
        if (!attackController) attackController = PlayerManager.Instance.attackController;
    }
    

    //버튼 함수
    public void OnHealStart()
    {
        SoundManager.Instance.PlaySfx(SoundManager.SfxId.Healing);
        playerCondition.AddHealth(playerCondition.GetHealth());
        playerCondition.RestoreMana(playerCondition.MaxMana);
        attackController.OnHealStart();
    }
    public void PlaySpinAttack()
    {
        SoundManager.Instance.PlaySfx(SoundManager.SfxId.SpinAttack);
        if (PlayerManager.Instance.playerCondition.Mana < attackController.ManaCost) return;
        attackController.animator.SetTrigger(attackController.SpinHash);
        PlayerManager.Instance.playerCondition.UseMana(attackController.ManaCost);
    }
}
