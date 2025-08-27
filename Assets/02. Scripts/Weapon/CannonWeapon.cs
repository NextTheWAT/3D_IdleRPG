using UnityEngine;

public class CannonWeapon : BaseWeapon
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject muzzleFlashPrefab;     // 머즐 플래쉬 파티클
    [SerializeField] private float muzzleFlashLifetime = 0.5f; // 파티클 자동 Destroy 미설정 시 안전망
    [SerializeField] private float muzzleFlashForwardOffset = 0.15f; // 총구 앞쪽으로 약간

    protected override void OnFire(Transform target)
    {
        if (projectilePrefab == null || firePoints == null || target == null) return;

        Vector3 targetPos = target.position;
        targetPos.y += 1.0f; // 살짝 위로 쏘기 보정

        // 레벨별 머즐 포인트 제한을 쓰고 싶다면 아래 한 줄로 제한 가능
        // int count = Mathf.Min(firePoints.Length, data.maxFirePoints);
        // for (int i = 0; i < count; i++)
        for (int i = 0; i < firePoints.Length; i++)
        {
            var fp = firePoints[i];
            if (fp == null) continue;

            Vector3 dir = (targetPos - fp.position).normalized;
            Quaternion rot = dir.sqrMagnitude > 0f ? Quaternion.LookRotation(dir, Vector3.up) : fp.rotation;

            // 1) 투사체
            var go = Instantiate(projectilePrefab, fp.position, rot);
            var proj = go.GetComponent<SimpleProjectile>();
            if (proj != null && data != null)
            {
                proj.Init(damage: runtimeDamage, speed: data.projectileSpeed, maxDistance: data.range);
            }

            // 2) 머즐 플래쉬 (발사 포인트 자식으로 붙임)
            if (muzzleFlashPrefab != null)
            {
                Vector3 flashPos = fp.position + fp.forward * muzzleFlashForwardOffset;
                var flash = Instantiate(muzzleFlashPrefab, flashPos, fp.rotation, fp);

                // 파티클에 Stop Action=Destroy가 안 돼있으면 안전망으로 제거
                var ps = flash.GetComponent<ParticleSystem>();
                if (ps == null || ps.main.stopAction != ParticleSystemStopAction.Destroy)
                {
                    Destroy(flash, muzzleFlashLifetime);
                }
            }
        }
    }
}
