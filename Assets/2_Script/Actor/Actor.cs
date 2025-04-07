using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Actor : MonoBehaviour
{
    [SerializeField] protected int maxHp = 10;  // 최대 생명력
    [SerializeField] protected int nowHp = 10;  // 현재 생명력

    [SerializeField] protected int attackDamage = 1;   // 공격력
    [SerializeField] protected float attackRate = 1f;  // 공격간격
    protected float nextAttackTime = 0f;  // 공격 후, 얼마나 시간이 지났는지 저장

    // MeleeAttack
    [SerializeField] protected float attackRange = 10f;    // 공격 거리
    [SerializeField] protected float attackAngle = 180f;  // 공격 각도

    // 공격 대상 태그 (해당 태그를 가진 오브젝트만 공격)
    [SerializeField] protected string targetTag = "Enemy";

    protected Vector3 moveVec;  // 이동할 방향
    [SerializeField] protected float moveSpeed;  // 이동 속도


    // 공격
    public virtual void Attack()
    {
        // 공격 실행
        MeleeBasicAttack();
    }


    void MeleeBasicAttack()
    {
        Debug.Log("공격 실행");

        // 태그 기반 검색을 위해 씬의 모든 게임 오브젝트 중 targetTag를 가진 것들을 찾음
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        // 감지된 모든 타겟 게임 오브젝트에 대해 반복
        foreach (GameObject target in targets)
        {
            // 자신과 타겟 사이의 거리 계산
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // 거리가 공격 반경 내에 있는지 확인
            if (distanceToTarget <= attackRange)
            {
                // 자신에서 타겟까지의 방향 벡터 계산
                Vector3 directionToTarget = target.transform.position - transform.position;
                directionToTarget.y = 0;  // Y축 값을 0으로 설정하여 수평면에서만 각도 계산 (높이 차이 무시)

                // 자신의 전방 벡터와 타겟 방향 벡터 사이의 각도 계산
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                // 계산된 각도가 공격 각도의 절반보다 작은지 확인
                if (angleToTarget <= attackAngle / 2)
                {
                    // Actor 컴포넌트가 있는지 확인하고 데미지 처리
                    Actor targetActor = target.GetComponent<Actor>();
                    if (targetActor != null)
                    {
                        targetActor.TakeDamage(attackDamage);
                        Debug.Log("공격 명중");
                        // 디버그 시각화: 공격이 적중한 타겟까지 빨간색 선 그리기
                        Debug.DrawLine(transform.position, target.transform.position, Color.red, 1f);
                    }
                }
            }
        }
    }


    // 피격
    public virtual void TakeDamage(int damage)
    {
        // 자식 클래스에서 구현할 내용
        nowHp -= damage;

        // 체력이 0 이하로 떨어지면 처리
        if (nowHp <= 0)
        {
            Die();
        }
    }

    // 사망 처리
    protected virtual void Die()
    {
        // 자식 클래스에서 구현할 내용
    }

    // 이동 메서드 (추상 메서드로 자식 클래스에서 반드시 구현해야 함)
    public abstract void Move();
}
