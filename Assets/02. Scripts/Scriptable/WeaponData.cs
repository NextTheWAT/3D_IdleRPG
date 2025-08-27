using UnityEngine;

public enum WeaponId { CannonLv1, CannonLv2, CannonLv3 }

[CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WD_Cannon")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public WeaponId weaponId;
    public string displayName;
    public Sprite icon;                  // ← UI 이미지에 사용

    [Header("Prefab & Firing")]
    public GameObject weaponPrefab;      // 캐논 프리팹
    public float damage = 10f;
    public float range = 10f;
    public float fireCooldown = 0.4f;
    public float projectileSpeed = 20f;
    public int maxFirePoints = 1;

    [Header("Progression")]
    public int maxLevelPerTier = 5;      // 예: 5레벨 찍으면 다음 티어로
    public WeaponData nextTier;          // 예: Lv1 → Lv2, Lv2 → Lv3

    [Header("Progression Tuning")]
    public float damagePerLevel = 1f;  // 업그레이드 1회당 +1 (원하면 SO별로 조절)

    [Header("VFX/SFX (optional)")]
    public GameObject upgradeVfxPrefab;  // 레벨업/티어전환 시 재생할 파티클
                                         // public AudioClip  upgradeSfx;
}
