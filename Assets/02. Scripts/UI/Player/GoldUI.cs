using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.onGoldChanged.AddListener(UpdateGoldUI);
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.onGoldChanged.RemoveListener(UpdateGoldUI);
    }

    private void UpdateGoldUI(int value)
    {
        if (goldText)
            goldText.text = "Gold : "+ value.ToString();
    }
}
