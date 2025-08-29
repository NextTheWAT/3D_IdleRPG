using System.Collections.Generic;
using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [Header("Heal Effects")]
    [SerializeField] private GameObject healParticlePrefab;  // ������ ��ƼŬ ������(Play On Awake, Loop On ����)

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
