using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject healParticlePrefab;  // ������ ��ƼŬ ������(Play On Awake, Loop On ����)
    [SerializeField] private Transform particleSpawnPoint;

    private GameObject healFxObj;

    public void OnHealStart()
    {
        healFxObj = Instantiate(healParticlePrefab, particleSpawnPoint);
        healFxObj.transform.localPosition = Vector3.zero;
        healFxObj.transform.localRotation = Quaternion.identity;

        }
    }

