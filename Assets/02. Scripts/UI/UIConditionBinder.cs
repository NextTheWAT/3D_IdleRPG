using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConditionBinder : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCondition player;

    [Header("HP UI")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private TMP_Text hpText;

    [Header("MP UI")]
    [SerializeField] private Slider mpBar;
    [SerializeField] private TMP_Text mpText;

    [Header("EXP UI")]
    [SerializeField] private Slider expBar;
    [SerializeField] private TMP_Text expText;

    private void OnEnable()
    {
        Debug.Log("UIConditionBinder OnEnable");
        player = player ? player : FindObjectOfType<PlayerCondition>();

        if (!player) return;

        // BaseCondition(HP) 이벤트 이름/시그니처는 프로젝트에 맞게 조정
        // 가정: BaseCondition에 onHealthChanged(float current, float normalized01)가 존재
        player.onHealthChanged.AddListener(OnHpChanged);

        player.onManaChanged.AddListener(OnMpChanged);
        player.onExpChanged.AddListener(OnExpChanged);

        // 초기값 보정(플레이 중 AddListener 뒤에도 바로 화면 최신화)
        // BaseCondition은 Awake에서 NotifyHealth()를 이미 수행하므로,
        // 여기서는 강제 갱신이 필요 없을 수 있지만 안전하게 한 번 더 받도록 설계.
        // => 이벤트 기반이므로 실제 초기 알림은 PlayerCondition.Awake/Notify*에서 이미 한 번 나감

        OnHpChanged(player.Health, player.Health01);
        OnMpChanged(player.Mana, player.Mana01);
        OnExpChanged(player.Exp, player.Exp01);
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
}
