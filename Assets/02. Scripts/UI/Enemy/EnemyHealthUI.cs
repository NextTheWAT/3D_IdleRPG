using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �� �Ӹ� �� ü�¹� + ������ ó��.
/// - Slider�� ���� ü��(0~1)�� �ݿ�
/// - ĵ������ ���� ī�޶� ���� ȸ��
/// - target�� ����(offset)�� ���� ��ġ ����
/// </summary>
public class EnemyHealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BaseCondition condition; // ������ �θ𿡼� �ڵ� Ž��
    [SerializeField] private Slider hpSlider;         // ĵ���� ���� Slider
    [SerializeField] private Transform target;        // ���� �� ��Ʈ(�Ǵ� �Ӹ� ��)

    [Header("Placement")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);

    [Header("Options")]
    [SerializeField] private bool hideWhenFull = false; // ü�� Ǯ�� �� �����
    [SerializeField] private bool hideWhenDead = true;  // ��� �� �����/��Ȱ��ȭ

    private Camera cam;


    private void Awake()
    {
        if (!condition) condition = GetComponentInParent<BaseCondition>();
        if (!target) target = condition ? condition.transform : transform.parent;
        cam = Camera.main;

        // Slider �ʱ�ȭ (������ ���)
        if (!hpSlider)
        {
            hpSlider = GetComponentInChildren<Slider>(true);
            if (!hpSlider) Debug.LogWarning("[EnemyHealthUI] hpSlider�� ����ֽ��ϴ�.", this);
        }
    }

    private void OnEnable()
    {
        // ���� �� �� �� ��� ����
        UpdateHealthBar();
        UpdatePositionAndFacing();
    }

    private void LateUpdate()
    {
        // �� ������ ���� (BaseCondition�� �̺�Ʈ�� ������ �ű⼭�� ȣ���ص� ��)
        UpdateHealthBar();
        UpdatePositionAndFacing();
    }

    private void UpdateHealthBar()
    {
        if (!condition || !hpSlider) return;

        float cur = Mathf.Max(0f, condition.GetHealth());
        float max = Mathf.Max(1f, condition.GetMaxHealth());
        float t = cur / max;
        hpSlider.value = t;

        // �����̴� ��
        hpSlider.value = t;

        // ���� �ɼ�
        if (hideWhenFull && t >= 0.999f) hpSlider.gameObject.SetActive(false);
        else hpSlider.gameObject.SetActive(true);

        if (hideWhenDead && t <= 0.0001f) gameObject.SetActive(false);
    }

    private void UpdatePositionAndFacing()
    {
        if (target)
            transform.position = target.position + worldOffset;

        if (!cam) cam = Camera.main;
        if (!cam) return;

        // ī�޶� �ٶ󺸴� ������ ȸ��(�ܼ�/����)
        Vector3 toCam = transform.position - cam.transform.position;
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    // BaseCondition�� �̺�Ʈ�� �ִٸ� �̷� ������ ���� ���� (����)
    // public void OnHealthChanged(float cur, float max) => UpdateHealthBar();
    // public void OnDied() { if (hideWhenDead) gameObject.SetActive(false); }
}
