using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 머리 위 체력바 + 빌보드 처리.
/// - Slider에 현재 체력(0~1)을 반영
/// - 캔버스를 메인 카메라를 향해 회전
/// - target의 위쪽(offset)에 따라 위치 고정
/// </summary>
public class EnemyHealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BaseCondition condition; // 없으면 부모에서 자동 탐색
    [SerializeField] private Slider hpSlider;         // 캔버스 안의 Slider
    [SerializeField] private Transform target;        // 보통 적 루트(또는 머리 뼈)

    [Header("Placement")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);

    [Header("Options")]
    [SerializeField] private bool hideWhenFull = false; // 체력 풀일 때 숨기기
    [SerializeField] private bool hideWhenDead = true;  // 사망 시 숨기기/비활성화

    private Camera cam;


    private void Awake()
    {
        if (!condition) condition = GetComponentInParent<BaseCondition>();
        if (!target) target = condition ? condition.transform : transform.parent;
        cam = Camera.main;

        // Slider 초기화 (없으면 경고만)
        if (!hpSlider)
        {
            hpSlider = GetComponentInChildren<Slider>(true);
            if (!hpSlider) Debug.LogWarning("[EnemyHealthUI] hpSlider가 비어있습니다.", this);
        }
    }

    private void OnEnable()
    {
        // 시작 시 한 번 즉시 갱신
        UpdateHealthBar();
        UpdatePositionAndFacing();
    }

    private void LateUpdate()
    {
        // 매 프레임 갱신 (BaseCondition에 이벤트가 있으면 거기서도 호출해도 됨)
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

        // 슬라이더 값
        hpSlider.value = t;

        // 숨김 옵션
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

        // 카메라를 바라보는 빌보드 회전(단순/안정)
        Vector3 toCam = transform.position - cam.transform.position;
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    // BaseCondition에 이벤트가 있다면 이런 식으로 연결 가능 (선택)
    // public void OnHealthChanged(float cur, float max) => UpdateHealthBar();
    // public void OnDied() { if (hideWhenDead) gameObject.SetActive(false); }
}
