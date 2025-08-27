using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UpgradeSlotView
{
    public Image icon;           // CannonX_Image ������ Image
    public TMP_Text levelText;   // CannonX_Image ������ Text (TMP)
    public Button upgradeBtn;    // CannonX_Image ������ Button
}

public class UpgradePanelController : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("�÷��̾�(��) ������Ʈ�� ���� WeaponSlotsManager")]
    public WeaponSlotsManager slotsManager;

    [Header("Cannon1, Cannon2, Cannon3 ������� ����")]
    public UpgradeSlotView[] views = new UpgradeSlotView[3];

    private void OnEnable()
    {
        BindAll();
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
                if (slotsManager == null) return;

                // ���׷��̵� (�Ŵ��� ���ο��� Ƽ�� ��ȯ/��ƼŬ���� ó��)
                slotsManager.Upgrade(idx);

                // UI ���ΰ�ħ
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

        // ��ư Ȱ��: ���� Ƽ��� �� �����ų�, ���� Ƽ� ������ true
        bool canMore = slot.level < slot.data.maxLevelPerTier || slot.data.nextTier != null;
        if (v.upgradeBtn) v.upgradeBtn.interactable = canMore;
    }
}
