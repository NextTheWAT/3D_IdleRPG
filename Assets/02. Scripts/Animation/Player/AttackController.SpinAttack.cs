using System.Collections.Generic;
using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [Header("Spin Attack Settings")]
    [SerializeField] private float spinHitRadius = 2.5f;
    [SerializeField] private int spinDamage = 20;
    [SerializeField] private float knockback = 8f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Effects")]
    [SerializeField] private GameObject hitParticlePrefab;   // ��ƼŬ ������
    [SerializeField] private Transform particleSpawnPoint;   // ��ƼŬ ���� ��ġ(Inspector���� ����)

    private bool isSpinning = false;
    private HashSet<Collider> hitThisSpin = new();

    // === �ִϸ��̼� �̺�Ʈ���� ȣ�� ===
    public void OnSpinStart()
    {
        isSpinning = true;
        hitThisSpin.Clear();
    }

    public void OnSpinEnd()
    {
        isSpinning = false;
        hitThisSpin.Clear();
    }

    private void FixedUpdate()
    {
        if (!isSpinning) return;

        Collider[] hits = (enemyMask.value != ~0)
            ? Physics.OverlapSphere(transform.position, spinHitRadius, enemyMask)
            : Physics.OverlapSphere(transform.position, spinHitRadius);

        foreach (var col in hits)
        {
            if (hitThisSpin.Contains(col)) continue;
            if (!col.CompareTag("Enemy")) continue;

            // --- ������ ���� (����/Ȱ��ȭ ����)
            // var dmg = col.GetComponent<IValueChangable>();
            // if (dmg != null) dmg.ValueChanged(-spinDamage);

            // --- ��ũ��
            Vector3 toEnemy = col.transform.position - transform.position;
            toEnemy.y = 0f;
            if (toEnemy.sqrMagnitude > 0.0001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, toEnemy).normalized;
                if (Random.value < 0.5f) side = -side;

                var rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>();
                if (rb != null && rb.isKinematic == false)
                {
                    rb.AddForce(side * knockback, ForceMode.VelocityChange);
                }
                else
                {
                    col.transform.position += side * (knockback * 0.05f);
                }
            }

            // --- ��ƼŬ ����
            if (hitParticlePrefab)
            {
                Vector3 spawnPos = particleSpawnPoint ? particleSpawnPoint.position : col.ClosestPoint(transform.position);
                Quaternion spawnRot = Quaternion.LookRotation(toEnemy != Vector3.zero ? toEnemy : Vector3.forward);
                Instantiate(hitParticlePrefab, spawnPos, spawnRot);
            }

            hitThisSpin.Add(col);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spinHitRadius);
    }
}
