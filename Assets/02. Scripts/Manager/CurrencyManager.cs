using System;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    [SerializeField] private int gold = 0;
    [SerializeField] private GoldUI goldUI; // GoldUI ���۷���

    // ��� ����Ǹ� UI�� ��� ������ �� �ֵ��� �̺�Ʈ ����
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (goldUI != null) goldUI.UpdateGoldUI(gold);
    }

    public int Gold => gold; // ���� ��� ��ȸ��

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        if (goldUI != null) goldUI.UpdateGoldUI(gold);
        OnGoldChanged?.Invoke(gold);  // �˸�
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        if (goldUI != null) goldUI.UpdateGoldUI(gold); // �Һ� �� UI �ݿ�
        OnGoldChanged?.Invoke(gold);  // �˸�
        return true;
    }
}
