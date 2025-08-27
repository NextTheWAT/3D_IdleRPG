using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] // 히트박스(Trigger) 전용
public class EnemyAttack : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask targetLayer = ~0;   // Player 레이어만 선택 권장

    [Header("Damage")]
    [SerializeField] private float contactDamage = 10f;    // 접촉 데미지
    [SerializeField] private float contactCooldown = 0.25f; // 동일 대상 재타격 최소 간격(초)

    [Header("Knockback (optional)")]
    [SerializeField] private bool applyKnockback = false;
    [SerializeField] private float knockbackForce = 5f;    // VelocityChange 기준

    [Header("Enemy")]
    [SerializeField] private GameObject enemyGameObject;

    EnemyCondition enemyCondition; 

    private void Awake()
    {
        enemyCondition = GetComponentInParent<EnemyCondition>();
    }


    private void OnTriggerEnter(Collider other)
    {
        // 태그/레이어 필터
        if (!other.CompareTag(playerTag)) return;
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        // 루트(또는 Rigidbody) 기준으로 동일 대상 판정
        Transform target = other.attachedRigidbody ? other.attachedRigidbody.transform
                                                   : (other.transform.root ? other.transform.root : other.transform);

        var dmg = target.GetComponent<IDamageable>();
        enemyCondition.DeathParticle();
        if (dmg == null) return;

        // 데미지
        dmg.TakeDamage(contactDamage);

        // 넉백(옵션)
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
