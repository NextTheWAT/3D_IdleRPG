using UnityEngine;

public enum WeaponId { CannonLv1, CannonLv2, CannonLv3 }

[CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WD_Cannon")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public WeaponId weaponId;
    public string displayName;
    public Sprite icon;                  // �� UI �̹����� ���

    [Header("Prefab & Firing")]
    public GameObject weaponPrefab;      // ĳ�� ������
    public float damage = 10f;
    public float range = 10f;
    public float fireCooldown = 0.4f;
    public float projectileSpeed = 20f;
    public int maxFirePoints = 1;

    [Header("Progression")]
    public int maxLevelPerTier = 5;      // ��: 5���� ������ ���� Ƽ���
    public WeaponData nextTier;          // ��: Lv1 �� Lv2, Lv2 �� Lv3

    [Header("Progression Tuning")]
    public float damagePerLevel = 1f;  // ���׷��̵� 1ȸ�� +1 (���ϸ� SO���� ����)

    [Header("VFX/SFX (optional)")]
    public GameObject upgradeVfxPrefab;  // ������/Ƽ����ȯ �� ����� ��ƼŬ
                                         // public AudioClip  upgradeSfx;
}
