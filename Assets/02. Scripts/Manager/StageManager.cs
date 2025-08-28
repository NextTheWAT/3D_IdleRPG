using System;
using UnityEngine;
using TMPro;

/// <summary>
/// ��������/Ÿ�̸�/UI ����. 3��(�⺻)���� Stage+1, �̺�Ʈ ����.
/// ���������� ���� ��ġ�� ���⼭ ����(EnemyManager�� �о� ���).
/// </summary>
public class StageManager : Singleton<StageManager>
{
    [Header("Stage")]
    [SerializeField] private int currentStage = 1;        // ���� ��������
    [SerializeField] private float stageDuration = 180f;  // ���������� 3��(�� ����)
    private float timeLeft;

    [Header("UI")]
    [SerializeField] private TMP_Text stageText;           // "Stage 1"
    [SerializeField] private TMP_Text timerText;           // "02:59"

    [Header("Per-Stage Buffs (���ϴ� ������ ����)")]
    [Tooltip("�������� 1�ܰ� ��´� HP ���귮(��: +25)")]
    public float hpAddPerStage = 0f;

    /// <summary>�������� ���� �� (�� �������� ��ȣ)</summary>
    public event Action<int> OnStageChanged;

    public int CurrentStage => currentStage;
    public float TimeLeft => Mathf.Max(0f, timeLeft);
    public float StageDuration => stageDuration;

    protected override void Awake()
    {
        base.Awake();
        timeLeft = stageDuration;
        UpdateStageUI();
        UpdateTimerUI();
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            NextStage();
        }
        UpdateTimerUI();
    }

    public void NextStage()
    {
        currentStage++;
        timeLeft = stageDuration;
        UpdateStageUI();
        OnStageChanged?.Invoke(currentStage);
    }

    private void UpdateStageUI()
    {
        if (stageText != null) stageText.text = $"Stage {currentStage}";
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        float t = Mathf.Max(0f, timeLeft);
        int mm = Mathf.FloorToInt(t / 60f);
        int ss = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{mm:00}:{ss:00}";
    }
}
