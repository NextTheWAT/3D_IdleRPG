using UnityEngine;

public class DebugConditionHotkeys : MonoBehaviour
{
    public PlayerCondition player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerCondition>();
    }

    void Update()
    {
        if (!player) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) player.ValueChanged(-10); // 데미지 10
        if (Input.GetKeyDown(KeyCode.Alpha2)) player.ValueChanged(+10); // 힐 10
        if (Input.GetKeyDown(KeyCode.Alpha3)) player.RestoreMana(5);    // 마나 +5
        if (Input.GetKeyDown(KeyCode.Alpha4)) player.UseMana(5);        // 마나 -5
        if (Input.GetKeyDown(KeyCode.Alpha5)) player.AddExp(25);        // EXP +25
    }
}
