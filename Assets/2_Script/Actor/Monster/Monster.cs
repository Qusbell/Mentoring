using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DamageReaction))]
abstract public class Monster : Actor
{
    private Transform _target;
    // 타겟
    public Transform target
    {
        get
        {
            if (_target == null)
            { _target = TargetManager.instance.target; }
            return _target;
        }
        set
        {
            _target = value;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작

        damageReaction.hitAnimation = () => SwitchStatus(HitStatus);
        damageReaction.dieAnimation = () => SwitchStatus(DieStatus);
    }


    private void Update()
    { actionStatus(); }


    // 공격 사거리 계산
    protected bool InAttackRange()
    { return (target.position - this.transform.position).sqrMagnitude <= attackAction.attackRange * attackAction.attackRange; }


    // 현재 수행 중인 행동
    protected Action actionStatus;


    // 애니메이션 트리거의 단일 활성화 보장
    protected bool animationTrigger = true;
    protected void PlayAnimationTriggerOnce(string animationName)
    {
        if (animationTrigger)
        {
            animationTrigger = false;
            animator.PlayAnimation(animationName);
        }
    }


    // 재생 중 애니메이션 확인
    protected bool animationPlayCheck = false;

    // 각종 상태 비활성화
    protected void SwitchStatus(Action nextStatus)
    {
        animationTrigger = true;
        animationPlayCheck = false;
        actionStatus = nextStatus;
    }

    // 애니메이션 종료 시, 다음 상태로 전환
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
        {
            SwitchStatus(AttackStatus);
        } // 공격으로
        else
        {
            SwitchStatus(MoveStatus);
        }  // 아니면 이동
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
    protected virtual void AttackStatus() // <- 하위 몬스터들도 제대로 확인할 것
    {
        // 공격 가능하다면
        if (attackAction.isCanAttack && InAttackRange())
        {
            // attackAction.Attack();
            PlayAnimationTriggerOnce("DoAttack");
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
        PlayAnimationTriggerOnce("IsHit");
        SwitchStatusWhenAnimationEnd("Hit", IdleStatus);
    }

    protected void DieStatus()
    {
        PlayAnimationTriggerOnce("IsDie");
    }
}