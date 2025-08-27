using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] // ��Ʈ�ڽ�(Trigger) ����
public class EnemyAttack : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask targetLayer = ~0;   // Player ���̾ ���� ����

    [Header("Damage")]
    [SerializeField] private float contactDamage = 10f;    // ���� ������
    [SerializeField] private float contactCooldown = 0.25f; // ���� ��� ��Ÿ�� �ּ� ����(��)

    [Header("Knockback (optional)")]
    [SerializeField] private bool applyKnockback = false;
    [SerializeField] private float knockbackForce = 5f;    // VelocityChange ����

    [Header("Enemy")]
    [SerializeField] private GameObject enemyGameObject;

    EnemyCondition enemyCondition; 

    private void Awake()
    {
        enemyCondition = GetComponentInParent<EnemyCondition>();
    }


    private void OnTriggerEnter(Collider other)
    {
        // �±�/���̾� ����
        if (!other.CompareTag(playerTag)) return;
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        // ��Ʈ(�Ǵ� Rigidbody) �������� ���� ��� ����
        Transform target = other.attachedRigidbody ? other.attachedRigidbody.transform
                                                   : (other.transform.root ? other.transform.root : other.transform);

        var dmg = target.GetComponent<IDamageable>();
        enemyCondition.DeathParticle();
        if (dmg == null) return;

        // ������
        dmg.TakeDamage(contactDamage);

        // �˹�(�ɼ�)
        if (applyKnockback)
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                dir.y = 0f;
                rb.AddForce(dir * knockbackForce, ForceMode.VelocityChange);
            }
        }
        Destroy(enemyGameObject, 0.1f);
    }
}
