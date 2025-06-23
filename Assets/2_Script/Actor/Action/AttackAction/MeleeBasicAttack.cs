using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//==================================================
// 근접 기본 공격
//==================================================
public class MeleeBasicAttack : AttackAction
{
    [SerializeField] protected float attackAngle = 90f;  // 공격 각도

    protected override void Awake()
    {
        base.Awake();
        doAttack = DoAttack;
    }



    protected void DoAttack()
    {
        // OverlapSphere를 사용하여 공격 범위 내의 모든 콜라이더를 찾음
        // <- 대상의 레이어를 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        // 감지한 모든 콜라이더에 대해서 판정
        foreach (Collider hitCollider in hitColliders)
        {
            // 타겟 태그를 가진 오브젝트인지 확인
            if (!hitCollider.CompareTag(targetTag)) { continue; }

            // 타겟 방향 벡터 계산
            Vector3 directionToTarget = hitCollider.transform.position - transform.position;
            directionToTarget.y = 0;  // Y축 값을 0으로 설정 (높이 차이 무시)
            
            // 자신의 전방 벡터와 타겟 방향 벡터 사이의 각도 계산
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            // 계산된 각도가 공격 각도의 절반보다 작은지 확인
            if (angleToTarget <= attackAngle / 2)
            {
                // DamageReaction 컴포넌트가 있는지 확인하고, 데미지 처리
                DamageReaction targetActor = hitCollider.GetComponent<DamageReaction>();
                if (targetActor != null)
                {
                    targetActor.TakeDamage(attackDamage);
                    // 디버그 시각화: 공격이 적중한 타겟까지 빨간색 선 그리기
                    Debug.DrawLine(transform.position, hitCollider.transform.position, Color.red, 1f);
                }
            }

        }
    } // MeleeBasicAttack


    // 공격 범위 시각화
    void OnDrawGizmos()
    {
        // 공격 범위를 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 공격 각도를 시각화
        Gizmos.color = Color.blue;
        Vector3 rightDir = Quaternion.Euler(0, attackAngle / 2, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -attackAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDir * attackRange);
        Gizmos.DrawRay(transform.position, leftDir * attackRange);
    }
}