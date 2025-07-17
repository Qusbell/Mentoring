using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



[RequireComponent (typeof (Rigidbody))]
abstract public class Actor : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 이동
    protected MoveAction moveAction;

    // 공격 <- 나중에 삭제 예정
    protected AttackAction attackAction;

    // 공격 목록
    // 각 공격행동이 자신의 키를 보유
    protected Dictionary<AttackName, AttackAction> attackActions = new Dictionary<AttackName, AttackAction>();


    // 피격
    protected DamageReaction damageReaction;

    // 애니메이션
    protected ActorAnimation animator;

    // 생성 초기화
    protected virtual void Awake()
    {
        // 물리연산 포함
        rigid = GetComponent<Rigidbody>();
        // 물리회전 제거
        rigid.freezeRotation = true;

        animator = GetComponent<ActorAnimation>();
        moveAction = GetComponent<MoveAction>();
        damageReaction = GetComponent<DamageReaction>();

        // 공격 목록 만들기
        AttackAction[] tempAttackActions = GetComponents<AttackAction>();
        foreach (var item in tempAttackActions)
        {
            if(attackActions.ContainsKey(item.attackName))
            { Debug.LogError("중복된 공격 Name 할당 : " + this.gameObject.name + " : " + item.attackName); continue; }
            else
            { attackActions[item.attackName] = item; }
        }


        // <- 차후 천천히 제거
        attackAction = GetComponent<AttackAction>();
    }
}