// CarSpeedUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarSpeedUI : MonoBehaviour
{
    [SerializeField] private CarAI car;          // 대상 CarAI
    [SerializeField] private TMP_Text speedText; // "123 km/h"

    void Awake()
    {
        if (!car) car = FindObjectOfType<CarAI>();
    }

    void Update()
    {
        if (!car) return;

        float kmh = car.CurrentSpeedKmh * 2f; //*2는 좀더 텍스트로 속도감을 높이기위해
        if (speedText) speedText.text = Mathf.RoundToInt(kmh) + " km/h"; 
    }
}
