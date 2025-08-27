using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UpgradeSlotView
{
    public Image icon;           // CannonX_Image 하위의 Image
    public TMP_Text levelText;   // CannonX_Image 하위의 Text (TMP)
    public Button upgradeBtn;    // CannonX_Image 하위의 Button
}

public class UpgradePanelController : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("플레이어(차) 오브젝트에 붙은 WeaponSlotsManager")]
    public WeaponSlotsManager slotsManager;

    [Header("Cannon1, Cannon2, Cannon3 순서대로 연결")]
    public UpgradeSlotView[] views = new UpgradeSlotView[3];

    private void OnEnable()
    {
        BindAll();
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
                if (slotsManager == null) return;

                // 업그레이드 (매니저 내부에서 티어 전환/파티클까지 처리)
                slotsManager.Upgrade(idx);

                // UI 새로고침
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

        // 버튼 활성: 현재 티어에서 더 오르거나, 다음 티어가 있으면 true
        bool canMore = slot.level < slot.data.maxLevelPerTier || slot.data.nextTier != null;
        if (v.upgradeBtn) v.upgradeBtn.interactable = canMore;
    }
}
