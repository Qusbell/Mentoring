using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 플레이어 정보를
// UI로 전달
public class SendPlayerInfoToUI : MonoBehaviour
{
    private DamageReaction damageReaction;
    private StaminaAction staminaAction;

    // 텍스트 바꾸기
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        damageReaction = GetComponent<DamageReaction>();
        staminaAction = GetComponent<StaminaAction>();
    }


    void Update()
    {
        statusText.text = $"HP: {damageReaction.healthPoint} | SP: {staminaAction.stamina}";
    }

}
