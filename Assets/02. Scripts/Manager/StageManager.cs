using System;
using UnityEngine;
using TMPro;

/// <summary>
/// 스테이지/타이머/UI 전담. 3분(기본)마다 Stage+1, 이벤트 발행.
/// 스테이지별 버프 수치는 여기서 설정(EnemyManager가 읽어 사용).
/// </summary>
public class StageManager : Singleton<StageManager>
{
    [Header("Stage")]
    [SerializeField] private int currentStage = 1;        // 시작 스테이지
    [SerializeField] private float stageDuration = 180f;  // 스테이지당 3분(초 단위)
    private float timeLeft;

    [Header("UI")]
    [SerializeField] private TMP_Text stageText;           // "Stage 1"
    [SerializeField] private TMP_Text timerText;           // "02:59"

    [Header("Per-Stage Buffs (원하는 값으로 조절)")]
    [Tooltip("스테이지 1단계 상승당 HP 가산량(예: +25)")]
    public float hpAddPerStage = 0f;

    /// <summary>스테이지 변경 시 (새 스테이지 번호)</summary>
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
