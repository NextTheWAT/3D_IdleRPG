// WeaponSlotsManager.cs (��ü ���� ����)
using UnityEngine;

[System.Serializable]
public class WeaponSlot
{
    public string slotName = "Cannon";
    public Transform mount;                 // ���� ����
    public BaseWeapon weaponInstance;
    public WeaponData data;
    public int level = 1;
}

public class WeaponSlotsManager : Singleton<WeaponSlotsManager>
{
    [Tooltip("Cannon1/2/3 ������� 3��")]
    public WeaponSlot[] slots = new WeaponSlot[3];

    // ���� VFX (���⺰ VFX�� �켱 ����ϰ�, ������ �̰� ���)
    [Header("VFX (optional)")]
    public GameObject defaultUpgradeVfxPrefab;

    private void Start()
    {
        InitializeSlots(); // ���� �� ���Ժ� ���� �̸� ����
    }

    public WeaponSlot GetSlot(int idx) => (idx >= 0 && idx < slots.Length) ? slots[idx] : null;

    public void InitializeSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.data == null || s.mount == null) continue;

            // �������� ���������� ����
            SpawnAtMount(s);
        }
    }

    public bool Upgrade(int idx)
    {
        var s = GetSlot(idx);
        if (s == null || s.data == null) return false;

        s.level++;  // ���� +1

        if (s.level > s.data.maxLevelPerTier)
        {
            if (s.data.nextTier != null)
            {
                s.data = s.data.nextTier;
                s.level = 1;
                ReplaceInstanceWithTierPrefab(s); // ������ ��ü
            }
            else
            {
                s.level = s.data.maxLevelPerTier;
                return false;
            }
        }

        // ���� ������ ���⿡ �ݿ� (damage = base + (level-1)*perLevel)
        if (s.weaponInstance != null)
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel);

        return true;
    }

    private void SpawnAtMount(WeaponSlot s)
    {
        if (s.data == null || s.data.weaponPrefab == null) return;
        var go = Instantiate(s.data.weaponPrefab, s.mount.position, s.mount.rotation, s.mount);
        s.weaponInstance = go.GetComponent<BaseWeapon>();
        if (s.weaponInstance != null)
        {
            s.weaponInstance.SetData(s.data);
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // �� �߰�
        }
    }

    private void ReplaceInstanceWithTierPrefab(WeaponSlot s)
    {
        if (s.weaponInstance == null || s.data == null || s.data.weaponPrefab == null) return;

        Transform parent = s.weaponInstance.transform.parent;
        Vector3 pos = s.weaponInstance.transform.position;
        Quaternion rot = s.weaponInstance.transform.rotation;

        Destroy(s.weaponInstance.gameObject);

        var go = Instantiate(s.data.weaponPrefab, pos, rot, parent);
        s.weaponInstance = go.GetComponent<BaseWeapon>();
        if (s.weaponInstance != null)
        {
            s.weaponInstance.SetData(s.data);
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // �� �߰�
        }
    }

    private void PlayUpgradeVfx(WeaponSlot s)
    {
        // 1) ���� �����Ϳ� ������ VFX �켱
        GameObject vfxPrefab = s.data != null && s.data.upgradeVfxPrefab != null
            ? s.data.upgradeVfxPrefab
            : defaultUpgradeVfxPrefab;
        if (vfxPrefab == null || s.weaponInstance == null) return;

        var t = s.weaponInstance.transform;
        var vfx = Instantiate(vfxPrefab, t.position, t.rotation, t); // ���⿡ �ڽ����� ���̱�
        // ��ƼŬ�� �ڵ� �ı�(Stop Action: Destroy) �����̸� �߰� �ڵ� ���ʿ�
        // �ƴ϶�� ���� ����ó�� ���� �ð� �� �ı�:
        // Destroy(vfx, 3f);
    }

}
