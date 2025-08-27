using UnityEngine;

public class CannonWeapon : BaseWeapon
{
    [SerializeField] private GameObject projectilePrefab;

    protected override void OnFire(Transform target)
    {
        if (projectilePrefab == null || firePoints == null || target == null) return;

        Vector3 targetPos = target.position;

        for (int i = 0; i < firePoints.Length; i++)
        {
            var fp = firePoints[i];
            if (fp == null) continue;

            Vector3 dir = (targetPos - fp.position).normalized;
            Quaternion rot = dir.sqrMagnitude > 0f ? Quaternion.LookRotation(dir, Vector3.up) : fp.rotation;

            var go = Instantiate(projectilePrefab, fp.position, rot);
            var proj = go.GetComponent<SimpleProjectile>();
            if (proj != null && data != null)
            {
                proj.Init(damage: runtimeDamage, speed: data.projectileSpeed, maxDistance: data.range);
            }
        }
    }


}
