using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Actor : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 불필요한 회전 물리 제거
    protected virtual void FreezeVelocity()
    { rigid.angularVelocity = Vector3.zero; }

    // 생성 시 초기화
    protected virtual void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        // null 초기화 방어
        if (rigid == null)
        {
            Debug.LogError("Rigidbody 컴포넌트 누락!", gameObject);
            enabled = false; // 생성 취소
        }
    }

    // 물리엔진과 함께 업데이트 (0.02s)
    protected virtual void FixedUpdate()
    {
        FreezeVelocity();
    }


    //==================================================
    // 이동/회전 메서드
    //==================================================
    #region 이동/회전 메서드

    // 이동할 방향
    protected Vector3 moveVec;
    // 이동 속도
    [SerializeField] protected float moveSpeed;


    // 이동 메서드
    // Update()에서 갱신
    public virtual void Move()
    {
        if (moveVec != Vector3.zero)
        {
            // 현재위치 += 방향 * 이동 간격 * 이동 간격 보정
            rigid.MovePosition(rigid.position
            + moveVec * moveSpeed * Time.deltaTime);
        }
    }

    // 회전
    // 진행 방향을 바라봄
    public virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }
    #endregion


    //==================================================
    // 공격 메서드
    //==================================================
    #region 공격 메서드

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



    // 공격
    public virtual void Attack()
    { Debug.Log("virtual Attack()이 실행됨"); }

    #endregion


    //==================================================
    // 피격/사망 메서드
    //==================================================
    #region 피격/사망 메서드

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



    // 피격 반응
    protected virtual void DamageReaction()
    {

    }


    // 사망 처리
    protected virtual void Die()
    {

    }
    #endregion

}