using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public PlayerCondition playerCondition;


    private void Awake()
    {
        if (!playerCondition) playerCondition = FindObjectOfType<PlayerCondition>();
    }

}
