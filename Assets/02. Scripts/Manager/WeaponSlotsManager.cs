// WeaponSlotsManager.cs (전체 갱신 예시)
using UnityEngine;

[System.Serializable]
public class WeaponSlot
{
    public string slotName = "Cannon";
    public Transform mount;                 // 생성 기준
    public BaseWeapon weaponInstance;
    public WeaponData data;
    public int level = 1;
}

public class WeaponSlotsManager : Singleton<WeaponSlotsManager>
{
    [Tooltip("Cannon1/2/3 순서대로 3개")]
    public WeaponSlot[] slots = new WeaponSlot[3];

    // 공용 VFX (무기별 VFX를 우선 사용하고, 없으면 이걸 사용)
    [Header("VFX (optional)")]
    public GameObject defaultUpgradeVfxPrefab;

    private void Start()
    {
        InitializeSlots(); // 시작 시 슬롯별 무기 미리 생성
    }

    public WeaponSlot GetSlot(int idx) => (idx >= 0 && idx < slots.Length) ? slots[idx] : null;

    public void InitializeSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.data == null || s.mount == null) continue;

            // 데이터의 프리팹으로 생성
            SpawnAtMount(s);
        }
    }

    public bool Upgrade(int idx)
    {
        var s = GetSlot(idx);
        if (s == null || s.data == null) return false;

        s.level++;  // 레벨 +1

        if (s.level > s.data.maxLevelPerTier)
        {
            if (s.data.nextTier != null)
            {
                s.data = s.data.nextTier;
                s.level = 1;
                ReplaceInstanceWithTierPrefab(s); // 프리팹 교체
            }
            else
            {
                s.level = s.data.maxLevelPerTier;
                return false;
            }
        }

        // 현재 레벨을 무기에 반영 (damage = base + (level-1)*perLevel)
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
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // ★ 추가
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
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // ★ 추가
        }
    }

    private void PlayUpgradeVfx(WeaponSlot s)
    {
        // 1) 무기 데이터에 지정된 VFX 우선
        GameObject vfxPrefab = s.data != null && s.data.upgradeVfxPrefab != null
            ? s.data.upgradeVfxPrefab
            : defaultUpgradeVfxPrefab;
        if (vfxPrefab == null || s.weaponInstance == null) return;

        var t = s.weaponInstance.transform;
        var vfx = Instantiate(vfxPrefab, t.position, t.rotation, t); // 무기에 자식으로 붙이기
        // 파티클이 자동 파괴(Stop Action: Destroy) 설정이면 추가 코드 불필요
        // 아니라면 다음 라인처럼 일정 시간 후 파괴:
        // Destroy(vfx, 3f);
    }

}
