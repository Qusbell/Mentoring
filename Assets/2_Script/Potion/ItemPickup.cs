using UnityEngine;

public enum ItemType
{
    HealthPotion,    // 체력 회복
    StaminaPotion    // 스태미나 회복
}

public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    public ItemType itemType = ItemType.HealthPotion;

    [Header("회복량 설정")]
    [SerializeField] private int healthHealAmount = 50;    // 체력 회복량
    [SerializeField] private int staminaHealAmount = 2;    // 스태미나 회복량

    [Header("효과")]
    public GameObject pickupEffect;  // 픽업 시 파티클 효과
    public AudioClip pickupSound;    // 픽업 시 사운드

    private void Awake()
    {
        // 아이템 태그 자동 설정
        if (!gameObject.CompareTag("Item"))
        {
            gameObject.tag = "Item";
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
        switch (itemType)
        {
            case ItemType.HealthPotion:
                DamageReaction damageReaction = player.GetComponent<DamageReaction>();
                if (damageReaction != null)
                {
                    damageReaction.Heal(healthHealAmount); // 새 메서드 사용
                    return true;
                }
                break;

            case ItemType.StaminaPotion:
                StaminaAction staminaAction = player.GetComponent<StaminaAction>();
                if (staminaAction != null)
                {
                    staminaAction.RecoverStamina(staminaHealAmount); // 새 메서드 사용
                    return true;
                }
                break;
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