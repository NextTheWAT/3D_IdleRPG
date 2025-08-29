using System.Collections.Generic;
using UnityEngine;

public partial class AttackController : MonoBehaviour
{
    [Header("Spin Attack Settings")]
    [SerializeField] private float spinHitRadius = 2.5f;
    [SerializeField] private float knockback = 8f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Spin Effects")]
    [SerializeField] private GameObject spinParticlePrefab;  // 루프용 파티클 프리팹(Play On Awake, Loop On 권장)
    [SerializeField] private Transform particleSpawnPoint;

    [SerializeField, HideInInspector] private bool isSpinning = false; // 인스펙터에서 실수로 체크 방지
    private HashSet<Collider> hitThisSpin = new();

    // 루프 파티클 캐시(한 번 만들고 재사용)
    private GameObject spinFxObj;
    private ParticleSystem spinPs;


    private void FixedUpdate()
    {
        if (!isSpinning) return;

        // 히트 체크(파티클 생성 코드는 제거!)
        var hits = (enemyMask.value != ~0)
            ? Physics.OverlapSphere(transform.position, spinHitRadius, enemyMask)
            : Physics.OverlapSphere(transform.position, spinHitRadius);

        foreach (var col in hits)
        {
            if (hitThisSpin.Contains(col)) continue;
            if (!col.CompareTag("Enemy")) continue;

            // 데미지/노크백만 수행
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

        // 스핀 시작할 때 파티클 생성/재생
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

        // 스핀 끝났을 때 정지(Disable이 필요하면 여기서 spinFxObj.SetActive(false)도 가능)
        if (spinPs)
            spinPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

}
