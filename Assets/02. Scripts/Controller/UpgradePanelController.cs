using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UpgradeSlotView
{
    public Image icon;           // CannonX_Image ������ Image
    public TMP_Text levelText;   // CannonX_Image ������ Text (TMP)
    public TMP_Text slotText;
    public Button upgradeBtn;    // CannonX_Image ������ Button
}

public class UpgradePanelController : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("�÷��̾�(��) ������Ʈ�� ���� WeaponSlotsManager")]
    public WeaponSlotsManager slotsManager;

    [Header("Cannon1, Cannon2, Cannon3 ������� ����")]
    public UpgradeSlotView[] views = new UpgradeSlotView[3];

    [SerializeField] private int upgradeCost = 50;

    private void OnEnable()
    {
        BindAll();
        RefreshAll();

        // ��� �ٲ� �� UI �ڵ� ����
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
    }
    private void HandleGoldChanged(int _)
    {
        RefreshAll();
    }
    /// <summary>
    /// �� ��ư Ŭ���ÿ� slotsManager.Upgrade(i) ȣ���ϵ��� ���ε�
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

                // ����: IsInitialized ���� �߰� ���� ���� �ٷ� �õ�
                if (!CurrencyManager.Instance.TrySpendGold(upgradeCost))
                {
                    // TODO: ���� ��� �˸�
                    return;
                }

                // ���׷��̵�
                bool ok = slotsManager.Upgrade(idx);
                if (!ok)
                {
                    // ���� �� ȯ���� �ʿ��ϸ� �Ʒ� �ּ� ����
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

        // ������
        if (v.icon) v.icon.sprite = slot.data.icon;

        // ���� �ؽ�Ʈ: "�̸�  Lv.X/Y"
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
            v.slotText.SetText($"Damage : {curDmg:0.#} �� {nextDmg:0.#}");
            // ������ �����ַ���:  v.slotText.SetText($"Damage : {curDmg:0.#} �� {nextDmg:0.#}   |   Cost : {upgradeCost}G");
        }

        bool canMore = slotsManager.CanUpgrade(idx);
        bool enoughGold = CurrencyManager.Instance != null &&
                          CurrencyManager.Instance.Gold >= upgradeCost;
        if (v.upgradeBtn) v.upgradeBtn.interactable = canMore && enoughGold;
    }
}
