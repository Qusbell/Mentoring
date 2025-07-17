using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DamageReaction))]
public class Monster : Actor
{
    private Transform _target;
    // 타겟
    public Transform target
    {
        get
        {
            if (_target == null)
            { _target = TargetManager.Instance.target; }
            return _target;
        }
        set
        {
            _target = value;
        }
    }

    // 추격 행동
    ChaseAction chaseAction;

    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작

        damageReaction.hitAnimation = () => SwitchStatus(HitStatus);
        damageReaction.dieAnimation = () => SwitchStatus(DieStatus);

        // 낙사 추가
        if(GetComponent<FallingAction>() == null)
        { this.AddComponent<FallingAction>(); }

        chaseAction = moveAction as ChaseAction;
        if (chaseAction == null) { Debug.Log(this.gameObject.name + " : ChaseAction 아님"); }

        TurnWhenStart();
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

    // 상태 변경 / 애니메이션 체크 초기화
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

    protected void TurnWhenStart()
    {
        Vector3 targetPos = target.position;
        targetPos.y = this.transform.position.y; // y값을 동일하게 고정
        this.transform.LookAt(targetPos); // 평면상에서만 타겟을 바라봄
    }





    // ===== 상태 =====

    // 생성 상태
    private void SpawnState()
    {
        // Debug.Log("Spawn");

        if (!animator.CheckAnimationName("Spawn")) // 스폰 애니메이션 종료 시
        { SwitchStatus(IdleStatus); }
    }


    // 대기 상태
    protected void IdleStatus()
    {
        // Debug.Log("Idle");

        if (InAttackRange() && chaseAction.IsFacingTarget())
        { SwitchStatus(AttackStatus); }
        else if (chaseAction.isCanChase)
        { SwitchStatus(MoveStatus); }
        else
        { moveAction.Turn(); }
    }


    // 이동 상태
    protected void MoveStatus()
    {
        // Debug.Log("Move");


        if (InAttackRange())
        {
            moveAction.isMove = false;

            if (chaseAction.IsFacingTarget())
            { SwitchStatus(AttackStatus); }
            else
            {
                moveAction.Turn();
            }
        }
        else if (chaseAction.isCanChase)
        {
            moveAction.isMove = true;
            moveAction.Move();
            moveAction.Turn();
        }
        else
        {
            moveAction.isMove = false;
            SwitchStatus(IdleStatus);
        }

        animator.PlayAnimation("IsMove", moveAction.isMove);
    }


    // <- 공격 선딜레이 상태?


    // 공격 상태
    protected virtual void AttackStatus()
    {
        // Debug.Log("Attack");

        // 공격 가능하다면
        if (attackAction.isCanAttack)
        {
            if (InAttackRange() && chaseAction.IsFacingTarget())
            {
                attackAction.Attack();
                PlayAnimationTriggerOnce("DoAttack");
            }
            else { SwitchStatus(IdleStatus); }
        }

        SwitchStatusWhenAnimationEnd("Attack", ReloadStatus);
    }


    // 공격 후딜레이 애니메이션 재생
    protected void ReloadStatus()
    {
        // Debug.Log("Reload");
        // Attack -> Reload는 Animator 창에서 지정됨
        SwitchStatusWhenAnimationEnd("Reload", IdleStatus);
    }

    // 피격 시 애니메이션

    protected void HitStatus()
    {
        // Debug.Log("Hit");
        PlayAnimationTriggerOnce("IsHit");
        SwitchStatusWhenAnimationEnd("Hit", IdleStatus);
    }

    protected void DieStatus()
    {
        // Debug.Log("Die");
        PlayAnimationTriggerOnce("IsDie");
    }
}