using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    public void UpdateGoldUI(int value)
    {
        if (goldText)
            goldText.text = "Gold : "+ value.ToString();
    }
}
