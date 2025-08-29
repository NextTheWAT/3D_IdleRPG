using System.Collections.Generic;
using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [Header("Heal Effects")]
    [SerializeField] private GameObject healParticlePrefab;  // 루프용 파티클 프리팹(Play On Awake, Loop On 권장)

    [SerializeField] public Animator animator;
    [SerializeField] private int manaCost = 20;
    private readonly int spinHash = Animator.StringToHash("Spin_Attack");

    public float ManaCost => manaCost;
    public int SpinHash => spinHash;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private GameObject healFxObj;

    public void OnHealStart()
    {
        healFxObj = Instantiate(healParticlePrefab, particleSpawnPoint);
        healFxObj.transform.localPosition = Vector3.zero;
        healFxObj.transform.localRotation = Quaternion.identity;

    }
}
