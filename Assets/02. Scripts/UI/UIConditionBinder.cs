using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConditionBinder : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TMP_Text hpText;

    [Header("MP UI")]
    [SerializeField] private Slider mpBar;
    [SerializeField] private TMP_Text mpText;

    [Header("EXP UI")]
    [SerializeField] private Slider expBar;
    [SerializeField] private TMP_Text expText;

    [Header("Level UI")]
    [SerializeField] private TMP_Text levelText;

    private PlayerCondition player;

    private void Awake()
    {
        // PlayerManager가 이미 떠 있으면 우선 사용
        if (PlayerManager.Instance != null)
            player = PlayerManager.Instance.playerCondition;
    }

    private void OnEnable()
    {
        if (!player) return;

        // 중복 구독 방지 위해 Enable 시 구독
        player.onHealthChanged.AddListener(OnHpChanged);
        player.onManaChanged.AddListener(OnMpChanged);
        player.onExpChanged.AddListener(OnExpChanged);
        player.onLeveledUp.AddListener(OnLevelChanged);

        // 초기값 즉시 반영
        OnHpChanged(player.Health, player.Health01);
        OnMpChanged(player.Mana, player.Mana01);
        OnExpChanged(player.Exp, player.Exp01);
        OnLevelChanged(player.Level);
    }

    private void OnHpChanged(float current, float normalized01)
    {
        Debug.Log($"OnHpChanged: current={current}, normalized01={normalized01}");
        if (hpBar) hpBar.value = normalized01;
        if (hpText) hpText.text = Mathf.RoundToInt(current).ToString();
    }

    private void OnMpChanged(float current, float normalized01)
    {
        if (mpBar) mpBar.value = normalized01;
        if (mpText) mpText.text = Mathf.RoundToInt(current).ToString();
    }

    private void OnExpChanged(float current, float normalized01)
    {
        if (expBar) expBar.value = normalized01;
        if (expText) expText.text = Mathf.RoundToInt(current).ToString();
    }

    private void OnLevelChanged(int level)
    {
        if (levelText && player)
        {
            levelText.text = $"Lv. {player.Level}";
        }
    }

}
