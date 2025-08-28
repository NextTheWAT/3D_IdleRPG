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

        bool didUpgrade = false;

        // 같은 티어 레벨업
        if (s.level < s.data.maxLevelPerTier)
        {
            s.level++;
            didUpgrade = true;
        }
        // 티어 업
        else if (s.data.nextTier != null)
        {
            s.data = s.data.nextTier;
            s.level = 1;
            ReplaceInstanceWithTierPrefab(s);   // 프리팹 교체 후
            didUpgrade = true;
        }
        else
        {
            return false; // 더 이상 업그레이드 없음
        }

        // 수치 반영
        if (s.weaponInstance != null)
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel);

        // 업그레이드 VFX 호출 (레벨/티어 업 모두 여기서 처리)
        if (didUpgrade) PlayUpgradeVfx(s);

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
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // 추가
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
            s.weaponInstance.ApplyLevel(s.level, s.data.damagePerLevel); // 추가
        }
    }
    public float GetDamageIfUpgraded(int idx)
    {
        var s = GetSlot(idx);
        if (s == null || s.data == null) return 0f;

        // 같은 티어에서 레벨업 가능한 경우
        if (s.level < s.data.maxLevelPerTier)
        {
            int nextLevel = s.level + 1;
            return s.data.damage + (nextLevel - 1) * s.data.damagePerLevel;
        }

        // 티어 업 가능한 경우(다음 티어 1레벨 기준)
        if (s.data.nextTier != null)
        {
            var next = s.data.nextTier;
            return next.damage; // 필요 시 next.damagePerLevel 등 규칙 추가
        }

        // 더 이상 상승 불가면 현재값 유지
        return GetCurrentDamage(idx);
    }
    public float GetCurrentDamage(int idx)
    {
        var s = GetSlot(idx);
        if (s == null || s.data == null) return 0f;

        int curLevel = Mathf.Max(1, s.level); // level이 1부터라면 그대로, 0부터면 +1 개념
        return s.data.damage + (curLevel - 1) * s.data.damagePerLevel;
    }

    public bool CanUpgrade(int idx)
    {
        var s = GetSlot(idx);
        if (s == null || s.data == null) return false;
        return s.level < s.data.maxLevelPerTier || s.data.nextTier != null;
    }
    private void PlayUpgradeVfx(WeaponSlot s)
    {
        GameObject vfxPrefab = (s.data != null && s.data.upgradeVfxPrefab != null)
            ? s.data.upgradeVfxPrefab
            : defaultUpgradeVfxPrefab;

        if (vfxPrefab == null || s.weaponInstance == null) return;

        Transform t = s.weaponInstance.transform;

        // 1) 월드 회전값을 "위쪽"으로 고정해 생성
        Quaternion worldRot = Quaternion.LookRotation(Vector3.up); // Z+가 위로 향하게

        // 2) 부모에 붙여도 되고(한 번만 재생이면 굳이 안 붙여도 OK)
        var go = Instantiate(vfxPrefab, t.position, worldRot, t); // 부모 붙이기 원치 않으면 ,t 제거

        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 3) 부모가 회전해도 파티클은 계속 위로 날도록 월드 시뮬레이션 권장
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Play();
        }

        Destroy(go, 3f);
    }

}
