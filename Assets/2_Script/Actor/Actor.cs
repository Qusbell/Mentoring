using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



// RequireComponent : attack / move
[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(DamageReaction))]
abstract public class Actor : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 불필요한 회전 물리 제거
    protected virtual void FreezeVelocity()
    { rigid.angularVelocity = Vector3.zero; }


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
        rigid = GetComponent<Rigidbody>();
        animatior = GetComponent<ActorAnimation>();

        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        damageReaction = GetComponent<DamageReaction>();
    }


    // 물리엔진과 함께 업데이트 (0.02s)
    protected virtual void FixedUpdate()
    { FreezeVelocity(); }
}