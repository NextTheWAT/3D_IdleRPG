using System.Collections.Generic;
using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [Header("Spin Attack Settings")]
    [SerializeField] private float spinHitRadius = 2.5f;
    [SerializeField] private float knockback = 8f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Spin Effects")]
    [SerializeField] private GameObject spinParticlePrefab;  // ������ ��ƼŬ ������(Play On Awake, Loop On ����)
    [SerializeField] private Transform particleSpawnPoint;

    [SerializeField, HideInInspector] private bool isSpinning = false; // �ν����Ϳ��� �Ǽ��� üũ ����
    private HashSet<Collider> hitThisSpin = new();

    // ���� ��ƼŬ ĳ��(�� �� ����� ����)
    private GameObject spinFxObj;
    private ParticleSystem spinPs;


    private void FixedUpdate()
    {
        if (!isSpinning) return;

        // ��Ʈ üũ(��ƼŬ ���� �ڵ�� ����!)
        var hits = (enemyMask.value != ~0)
            ? Physics.OverlapSphere(transform.position, spinHitRadius, enemyMask)
            : Physics.OverlapSphere(transform.position, spinHitRadius);

        foreach (var col in hits)
        {
            if (hitThisSpin.Contains(col)) continue;
            if (!col.CompareTag("Enemy")) continue;

            // ������/��ũ�鸸 ����
            Vector3 toEnemy = col.transform.position - transform.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude > 0.0001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, toEnemy).normalized;
                if (Random.value < 0.5f) side = -side;

                var rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>();
                if (rb && !rb.isKinematic)
                    rb.AddForce(side * knockback, ForceMode.VelocityChange);
                else
                    col.transform.position += side * (knockback * 0.05f);
            }

            hitThisSpin.Add(col);
        }
    }

    //Animation Event
    public void OnSpinStart()
    {
        isSpinning = true;
        hitThisSpin.Clear();

        // ���� ������ �� ��ƼŬ ����/���
        if (spinParticlePrefab && particleSpawnPoint)
        {
            if (!spinFxObj)
            {
                spinFxObj = Instantiate(spinParticlePrefab, particleSpawnPoint);
                spinFxObj.transform.localPosition = Vector3.zero;
                spinFxObj.transform.localRotation = Quaternion.identity;
                spinPs = spinFxObj.GetComponent<ParticleSystem>();
            }

            if (!spinFxObj.activeSelf) spinFxObj.SetActive(true);
            if (spinPs)
            {
                spinPs.Clear(true);
                spinPs.Play(true);
            }
        }
    }

    public void OnSpinEnd()
    {
        isSpinning = false;
        hitThisSpin.Clear();

        // ���� ������ �� ����(Disable�� �ʿ��ϸ� ���⼭ spinFxObj.SetActive(false)�� ����)
        if (spinPs)
            spinPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

}
