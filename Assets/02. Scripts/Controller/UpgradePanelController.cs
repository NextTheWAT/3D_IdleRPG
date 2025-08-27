using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UpgradeSlotView
{
    public Image icon;           // CannonX_Image 하위의 Image
    public TMP_Text levelText;   // CannonX_Image 하위의 Text (TMP)
    public TMP_Text slotText;
    public Button upgradeBtn;    // CannonX_Image 하위의 Button
}

public class UpgradePanelController : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("플레이어(차) 오브젝트에 붙은 WeaponSlotsManager")]
    public WeaponSlotsManager slotsManager;

    [Header("Cannon1, Cannon2, Cannon3 순서대로 연결")]
    public UpgradeSlotView[] views = new UpgradeSlotView[3];

    [SerializeField] private int upgradeCost = 50;

    private void OnEnable()
    {
        BindAll();
        RefreshAll();

        // 골드 바뀔 때 UI 자동 갱신
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
    }
    private void HandleGoldChanged(int _)
    {
        RefreshAll();
    }
    /// <summary>
    /// 각 버튼 클릭시에 slotsManager.Upgrade(i) 호출하도록 바인딩
    /// </summary>
    public void BindAll()
    {
        if (views == null) return;

        for (int i = 0; i < views.Length; i++)
        {
            int idx = i;
            if (views[i]?.upgradeBtn == null) continue;

            views[i].upgradeBtn.onClick.RemoveAllListeners();
            views[i].upgradeBtn.onClick.AddListener(() =>
            {
                if (slotsManager == null || !slotsManager.CanUpgrade(idx)) return;

                // 결제: IsInitialized 같은 추가 조건 없이 바로 시도
                if (!CurrencyManager.Instance.TrySpendGold(upgradeCost))
                {
                    // TODO: 부족 골드 알림
                    return;
                }

                // 업그레이드
                bool ok = slotsManager.Upgrade(idx);
                if (!ok)
                {
                    // 실패 시 환불이 필요하면 아래 주석 해제
                    // CurrencyManager.Instance.AddGold(upgradeCost);
                }

                Refresh(idx);
            });
        }
    }

    public void RefreshAll()
    {
        if (views == null) return;
        for (int i = 0; i < views.Length; i++) Refresh(i);
    }

    private void Refresh(int idx)
    {
        if (slotsManager == null || views == null || idx < 0 || idx >= views.Length) return;

        var v = views[idx];
        var slot = slotsManager.GetSlot(idx);

        if (slot == null || slot.data == null)
        {
            if (v.icon) v.icon.sprite = null;
            if (v.levelText) v.levelText.text = "-";
            if (v.upgradeBtn) v.upgradeBtn.interactable = false;
            return;
        }

        // 아이콘
        if (v.icon) v.icon.sprite = slot.data.icon;

        // 레벨 텍스트: "이름  Lv.X/Y"
        if (v.levelText)
        {
            int cur = Mathf.Max(1, slot.level);
            int max = Mathf.Max(1, slot.data.maxLevelPerTier);
            v.levelText.text = $"{slot.data.displayName}  Lv.{cur}/{max}";
        }
        if (v.slotText)
        {
            float curDmg = slotsManager.GetCurrentDamage(idx);
            float nextDmg = slotsManager.GetDamageIfUpgraded(idx);
            v.slotText.SetText($"Damage : {curDmg:0.#} → {nextDmg:0.#}");
            // 비용까지 보여주려면:  v.slotText.SetText($"Damage : {curDmg:0.#} → {nextDmg:0.#}   |   Cost : {upgradeCost}G");
        }

        bool canMore = slotsManager.CanUpgrade(idx);
        bool enoughGold = CurrencyManager.Instance != null &&
                          CurrencyManager.Instance.Gold >= upgradeCost;
        if (v.upgradeBtn) v.upgradeBtn.interactable = canMore && enoughGold;
    }
}
