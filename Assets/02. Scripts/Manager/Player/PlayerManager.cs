using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] public PlayerCondition playerCondition;
    [SerializeField] public AttackController attackController;
    [SerializeField] public CarAI carAI;
    [SerializeField] public Transform carTransform;
    private void Awake()
    {
        if (!playerCondition) playerCondition = FindObjectOfType<PlayerCondition>();
        if (!attackController) attackController = FindObjectOfType<AttackController>();
        if (!carAI) carAI = FindObjectOfType<CarAI>();
        if (!carTransform) carTransform = carAI?.transform;
    }
}
