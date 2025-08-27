using System;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    [SerializeField] private int gold = 0;
    [SerializeField] private GoldUI goldUI; // GoldUI 레퍼런스

    // 골드 변경되면 UI가 즉시 반응할 수 있도록 이벤트 발행
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (goldUI != null) goldUI.UpdateGoldUI(gold);
    }

    public int Gold => gold; // 현재 골드 조회용

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        if (goldUI != null) goldUI.UpdateGoldUI(gold);
        OnGoldChanged?.Invoke(gold);  // 알림
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        if (goldUI != null) goldUI.UpdateGoldUI(gold); // 소비 시 UI 반영
        OnGoldChanged?.Invoke(gold);  // 알림
        return true;
    }
}
