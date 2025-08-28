using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    [SerializeField] public PlayerCondition playerCondition;
    [SerializeField] private PlayerSkill PlayerSkill;


    private void Awake()
    {
        if (!playerCondition) playerCondition = FindObjectOfType<PlayerCondition>();
        if (!PlayerSkill) PlayerSkill = FindObjectOfType<PlayerSkill>();
    }
    

    public void OnHealStart()
    {
        SoundManager.Instance.PlaySfx(SoundManager.SfxId.Healing);
        playerCondition.AddHealth(playerCondition.GetHealth());
        playerCondition.RestoreMana(playerCondition.MaxMana);
        PlayerSkill.OnHealStart();
    }
}
