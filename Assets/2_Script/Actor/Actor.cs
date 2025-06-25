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
    // 공격
    protected AttackAction attackAction;
    // 피격
    protected DamageReaction damageReaction;

    // 애니메이션
    protected ActorAnimation animatior;

    // 생성 초기화
    protected virtual void Awake()
    {
        // 물리연산 포함
        rigid = GetComponent<Rigidbody>();
        // 물리회전 제거
        rigid.freezeRotation = true;

        animatior = GetComponent<ActorAnimation>();
        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        damageReaction = GetComponent<DamageReaction>();
    }
}