using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DamageReaction))]
abstract public class Monster : Actor
{
    // 타겟
    Transform target;


    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작

        damageReaction.hitAction = () => SwitchStatus(HitStatus);
        damageReaction.dieAction = () => SwitchStatus(DieStatus);
    }

    private void Start()
    { target = TargetManager.instance.target; }

    private void Update()
    { actionStatus(); }


    // 공격 사거리 계산
    protected bool InAttackRange()
    { return (target.position - this.transform.position).sqrMagnitude <= attackAction.attackRange * attackAction.attackRange; }


    // 현재 수행 중인 행동
    protected Action actionStatus;

    // 공격, 리로드 등 애니메이션 재생 추가 확인
    protected bool animationPlayCheck = false;

    protected void SwitchStatus(Action nextStatus)
    {
        animationPlayCheck = false;
        actionStatus = nextStatus;
    }

    protected void SwitchStatusWhenAnimationEnd(string animationName, Action nextStatus)
    {
        if (animator.CheckAnimationName(animationName))
        { animationPlayCheck = true; }
        else if (animationPlayCheck)
        { SwitchStatus(nextStatus); }
    }


    // ===== 상태 =====

    // 생성 상태
    private void SpawnState()
    {
        if (!animator.CheckAnimationName("Spawn")) // 스폰 애니메이션 종료 시
        { SwitchStatus(IdleStatus); } // 대기
    }

    // 대기 상태
    protected void IdleStatus()
    {
        if (InAttackRange())  // 공격 가능 상태라면
        { SwitchStatus(AttackStatus); } // 공격으로
        else
        { SwitchStatus(MoveStatus); }  // 아니면 이동
    }


    // 이동 상태
    protected void MoveStatus()
    {
        if (InAttackRange())
        {
            moveAction.isMove = false;
            SwitchStatus(AttackStatus);
        }
        else
        {
            moveAction.isMove = true;
            moveAction.Move();
        }

        animator.PlayAnimation("IsMove", moveAction.isMove);
    }



    // 공격 상태
    protected virtual void AttackStatus()
    {
        // 공격 가능하다면
        if (attackAction.isCanAttack && !animationPlayCheck && InAttackRange())
        {
            // attackAction.Attack();
            animator.PlayAnimation("DoAttack"); // 어택 애니메이션 재생
        }

        SwitchStatusWhenAnimationEnd("Attack", ReloadStatus);
    }

    // 공격 후딜레이 애니메이션 재생
    protected void ReloadStatus()
    {
        SwitchStatusWhenAnimationEnd("Reload", IdleStatus);
    }




    // 피격 시 애니메이션

    protected void HitStatus()
    {
        if (!animationPlayCheck)
        {
            animator.PlayAnimation("IsHit");
        }
        SwitchStatusWhenAnimationEnd("IsHit", IdleStatus);
    }


    protected void DieStatus()
    {
        if (!animationPlayCheck)
        {
            animator.PlayAnimation("IsDie");
        }
    }


}