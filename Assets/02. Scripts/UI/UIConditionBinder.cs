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

        // BaseCondition(HP) �̺�Ʈ �̸�/�ñ״�ó�� ������Ʈ�� �°� ����
        // ����: BaseCondition�� onHealthChanged(float current, float normalized01)�� ����
        player.onHealthChanged.AddListener(OnHpChanged);

        player.onManaChanged.AddListener(OnMpChanged);
        player.onExpChanged.AddListener(OnExpChanged);

        // �ʱⰪ ����(�÷��� �� AddListener �ڿ��� �ٷ� ȭ�� �ֽ�ȭ)
        // BaseCondition�� Awake���� NotifyHealth()�� �̹� �����ϹǷ�,
        // ���⼭�� ���� ������ �ʿ� ���� �� ������ �����ϰ� �� �� �� �޵��� ����.
        // => �̺�Ʈ ����̹Ƿ� ���� �ʱ� �˸��� PlayerCondition.Awake/Notify*���� �̹� �� �� ����

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
