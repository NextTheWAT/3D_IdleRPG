using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int gold = 0;
    public UnityEvent<int> onGoldChanged = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        onGoldChanged.Invoke(CurrentGold); // 시작 시 초기값 알림
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold += amount;
        onGoldChanged.Invoke(gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        onGoldChanged.Invoke(gold);
        return true;
    }


    public int CurrentGold => gold;
}
