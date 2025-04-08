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
    [SerializeField] protected string targetTag;

    protected Vector3 moveVec;  // 이동할 방향
    [SerializeField] protected float moveSpeed;  // 이동 속도




    // 공격
    public virtual void Attack()
    { Debug.Log("virtual Attack()이 실행됨"); }


    // 피격
    public virtual void TakeDamage(int damage)
    {
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }

        // 체력이 0 이하로 떨어지면 처리
        if (nowHp <= 0)
        { Die(); }
    }

    // 사망 처리
    protected virtual void Die()
    {
        // 자식 클래스에서 구현할 내용
    }

    // 이동 메서드
    public virtual void Move()
    { Debug.Log("virtual Move()가 실행됨"); }
}
