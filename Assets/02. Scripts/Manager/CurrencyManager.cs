using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int gold = 0;
    public UnityEvent<int> onGoldChanged;
    
    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        onGoldChanged?.Invoke(gold);
    }

    public void AddGold(int amount)
    {
        if (amount == 0) return;
        gold = Mathf.Max(0, gold + amount);
        onGoldChanged?.Invoke(gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        onGoldChanged?.Invoke(gold);
        return true;
    }

    public int CurrentGold => gold;
}
