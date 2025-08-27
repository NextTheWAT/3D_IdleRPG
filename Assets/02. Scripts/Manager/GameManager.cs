using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public PlayerCondition playerCondition;




    public void PlayerHeal()
    {
        playerCondition.AddHealth(playerCondition.GetHealth());
        playerCondition.RestoreMana(playerCondition.MaxMana);
    }
}
