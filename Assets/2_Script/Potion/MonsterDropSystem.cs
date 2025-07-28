using UnityEngine;

public class MonsterDropSystem : MonoBehaviour
{
    [Header("드롭할 포션들")]
    public GameObject healthPotionPrefab;   // 체력 포션 프리팹
    public GameObject staminaPotionPrefab;  // 스태미나 포션 프리팹

    [Header("드롭 설정")]
    public bool dropHealthPotion = false;    // 체력 포션 드롭 여부
    public bool dropStaminaPotion = false;   // 스태미나 포션 드롭 여부

    [Header("물리 설정")]
    public float dropHeight = 0.5f;         // 드롭 높이
    public float scatterRadius = 1f;        // 흩어짐 반경
    public Vector2 bounceForce = new Vector2(2f, 5f); // 튀어나오는 힘 (min, max)

    // 몬스터 사망 시 호출 - 체크된 포션들 무조건 드롭
    public void DropPotions()
    {
        Vector3 basePosition = transform.position + Vector3.up * dropHeight;

        // 체력 포션 드롭
        if (dropHealthPotion && healthPotionPrefab != null)
        {
            CreateDroppedItem(healthPotionPrefab, basePosition + Vector3.left * 0.3f);
        }

        // 스태미나 포션 드롭  
        if (dropStaminaPotion && staminaPotionPrefab != null)
        {
            CreateDroppedItem(staminaPotionPrefab, basePosition + Vector3.right * 0.3f);
        }

        Debug.Log($"{gameObject.name}이(가) 포션을 드롭");
    }

    private void CreateDroppedItem(GameObject itemPrefab, Vector3 position)
    {
        // 아이템 생성
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        // Item 태그 확실히 설정 (혹시 프리팹에서 빠졌을 경우 대비)
        if (!droppedItem.CompareTag("Item"))
        {
            droppedItem.tag = "Item";
            Debug.Log($"드롭된 {itemPrefab.name}에 Item 태그가 자동으로 설정.");
        }

        // 아이템을 Item 레이어로 설정 (몬스터와 충돌 방지용)
        SetLayerRecursively(droppedItem, LayerMask.NameToLayer("Item"));

        // 튀어나오는 효과
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-0.5f, 0.5f),
                1f,
                Random.Range(-0.5f, 0.5f)
            ).normalized;

            float force = Random.Range(bounceForce.x, bounceForce.y);
            rb.AddForce(randomDirection * force, ForceMode.Impulse);
        }
    }

    // 재귀적으로 레이어 변경 (자식 오브젝트들도 포함)
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}