using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class DropItem : MonoBehaviour
{
    [Header("회복량 설정")]
    [SerializeField] private int healthHealAmount = 50;    // 체력 회복량
    [SerializeField] private int staminaHealAmount = 2;    // 스태미나 회복량

    [Header("효과")]
    public GameObject pickupEffect;  // 픽업 시 파티클 효과
    public AudioClip pickupSound;    // 픽업 시 사운드

    private void Awake()
    {
        // 아이템 태그 안넣었을 때
        if (!gameObject.CompareTag("Item"))
        {
            Debug.LogWarning($"{this.gameObject.name} : 아이템의 태그가 일치하지 않음 (현재 : {this.gameObject.tag})");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (ApplyItemEffect(other.gameObject))
            {
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }

    private bool ApplyItemEffect(GameObject player)
    {

        DamageReaction damageReaction = player.GetComponent<DamageReaction>();
        StaminaAction staminaAction = player.GetComponent<StaminaAction>();

        if (damageReaction != null && staminaAction != null)
        {
            damageReaction.Heal(healthHealAmount); // 새 메서드 사용
            staminaAction.RecoverStamina(staminaHealAmount); // 새 메서드 사용
            return true;
        }

        return false;
    }


    private void PlayPickupEffects()
    {
        // 파티클 효과
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // 사운드 효과
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }
}