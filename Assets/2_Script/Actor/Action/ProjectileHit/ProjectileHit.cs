using UnityEngine;


public class ProjectileHit : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Player";

    private void Awake()
    {
        // 콜라이더 트리거 on
        Collider collider = GetComponent<Collider>();
        if (collider != null) { collider.isTrigger = true; }
        else { Debug.Log("Projectile에 콜라이더 없음"); }
    }


    // 콜라이더 필수 트리거
    private void OnTriggerEnter(Collider other)
    {
        // 태그 검증
        if (other.CompareTag(targetTag))
        {
            ApplyDamage(other.gameObject);
            Destroy(gameObject);
        }
        // 큐브 명중 시
        else if (other.CompareTag("Cube"))
        { Destroy(gameObject); } 
    }


    // 데미지 적용
    private void ApplyDamage(GameObject target)
    {
        // 예시: Health 컴포넌트가 있다고 가정
        DamageReaction health = target.GetComponent<DamageReaction>();
        if (health != null)
        { health.TakeDamage(damage); }
    }
}
