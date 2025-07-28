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
    [SerializeField] private int healthHealAmount = 10;    // 체력 회복량
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
            Debug.Log($"{gameObject.name}에 Item 태그가 자동으로 설정됨.");
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
                    damageReaction.healthPoint += healthHealAmount;
                    return true; // 항상 포션 사라짐
                }
                break;

            case ItemType.StaminaPotion:
                StaminaAction staminaAction = player.GetComponent<StaminaAction>();
                if (staminaAction != null)
                {
                    staminaAction.stamina += staminaHealAmount;
                    return true; // 항상 포션 사라짐
                }
                break;
        }

        return false; // 컴포넌트를 찾지 못한 경우만 사라지지 않음
    }
}