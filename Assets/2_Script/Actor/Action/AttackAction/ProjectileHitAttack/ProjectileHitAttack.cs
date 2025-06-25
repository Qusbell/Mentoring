using UnityEngine;


// 자신의 콜라이더를 참조하는 공격
// 투사체의 작동
public class ProjectileHitAttack : AttackAction
{
    protected override void Awake()
    {
        base.Awake();

        // 콜라이더 트리거 on
        Collider collider = GetComponent<Collider>();
        if (collider != null) { collider.isTrigger = true; }
        else { Debug.Log("Projectile에 콜라이더 없음"); }
    }


    // 콜라이더 필수 트리거
    private void OnTriggerEnter(Collider other)
    {
        // 대상 태그 적중 시
        // 데미지 적용 및 삭제
        if (other.CompareTag(targetTag))
        {
            ApplyDamage(other.gameObject);
            Destroy(gameObject);
        }
        // 큐브 충돌 시
        else if (other.CompareTag("Cube"))
        {
            Destroy(gameObject); // 삭제
        }
    }


    // 데미지 적용
    private void ApplyDamage(GameObject target)
    {
        // 예시: Health 컴포넌트가 있다고 가정
        DamageReaction health = target.GetComponent<DamageReaction>();
        if (health != null)
        { health.TakeDamage(attackDamage); }
    }
}