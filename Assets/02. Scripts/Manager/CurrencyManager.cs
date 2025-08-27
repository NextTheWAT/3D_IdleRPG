using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int gold = 0;
    [SerializeField] private GoldUI goldUI; // GoldUI 레퍼런스

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        goldUI.UpdateGoldUI(gold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        goldUI.UpdateGoldUI(gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }

}
