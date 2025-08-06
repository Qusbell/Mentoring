using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
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
        { _target = value; }
    }

    // 추격 행동
    protected ChaseAction chaseAction;



    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작

        // --- hit/die 등록 ---
        System.Action hitAction = () => { SwitchStatus(HitStatus); };
        System.Action dieAction = () => { SwitchStatus(DieStatus); };
        damageReaction.whenHit.AddMulti(hitAction);
        damageReaction.whenDie.AddOnce(dieAction);

        // --- 낙사 추가 ---
        if (GetComponent<FallingAction>() == null)
        { this.AddComponent<DestroyWhenFallingAction>(); }

        // moveAction에서 chaseAction 분리
        chaseAction = moveAction as ChaseAction;
        if (chaseAction == null) { Debug.Log(this.gameObject.name + " : ChaseAction 아님"); }
    }


    protected void Start()
    {
        // ---
        // 시작 시 1회, 타겟 방향 바라봄 (TurnToTargetWhenStart)
        Vector3 targetPos = target.position;
        targetPos.y = this.transform.position.y; // y값을 동일하게 고정
        this.transform.LookAt(targetPos);        // 평면상에서만 타겟을 바라봄
        // ---
    }


    // 현재 수행 중인 행동
    protected System.Action actionStatus;

    // (디버그용) 현재 상태 확인
    [SerializeField] private string currentStateName;
    private void UpdateStateName(Action state)
    { currentStateName = state.Method.Name; }

    private void Update()
    { actionStatus(); }



    // 트리거 애니메이션의 단일 활성화 보장
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

        // 디버그
        UpdateStateName(nextStatus);
    }

    // 애니메이션 종료 시, 다음 상태로 전환
    protected void SwitchStatusWhenAnimationEnd(string animationName, Action nextStatus)
    {
        if (animator.CheckAnimationName(animationName))
        { animationPlayCheck = true; }
        else if (animationPlayCheck)
        { SwitchStatus(nextStatus); }
    }



    // 공격 사거리 계산
    protected bool CheckInAttackRange()
    { return (target.position - this.transform.position).sqrMagnitude <= attackAction.attackRange * attackAction.attackRange; }

    // 공격 준비 완료
    protected bool isReadyToAttack
    {
        get
        { return attackAction.isCanAttack && CheckInAttackRange() && chaseAction.IsFacingTarget(attackAction.attackRange); }
    }



    // ===== 상태 =====

    // 생성 상태
    private void SpawnState()
    {
        if (!animator.CheckAnimationName("Spawn")) // 스폰 애니메이션 종료 시
        { SwitchStatus(IdleStatus); }
    }


    // 대기 상태
    protected void IdleStatus()
    {
        if (isReadyToAttack)
        { SwitchStatus(AttackStatus); }
        else if (!attackAction.isCanAttack)
        { moveAction.Turn(); }
        else
        { SwitchStatus(MoveStatus); }
    }


    // 이동 상태
    protected void MoveStatus()
    {
        if (isReadyToAttack)
        {
            moveAction.isMove = false;
            SwitchStatus(AttackStatus);
        }
        else
        {
            // 1회만 true로 변경 (임시)
            // 이유: chaseAction.isMove 변경 시에 타이머 발생
            if (!moveAction.isMove)
            { moveAction.isMove = true; }
            moveAction.Move();
            moveAction.Turn();
        }

        animator.PlayAnimation("IsMove", moveAction.isMove);
    }


    // 공격 상태
    protected virtual void AttackStatus()
    {
        // 공격 가능하다면
        if (isReadyToAttack)
        {
            attackAction.Attack();
            PlayAnimationTriggerOnce("DoAttack");
            SwitchStatus(AttackAnimationStatus);
        }
        // 아니면 Idle로
        else { SwitchStatus(IdleStatus); }
    }

    // 공격 애니메이션 상태
    protected void AttackAnimationStatus()
    { SwitchStatusWhenAnimationEnd("Attack", ReloadStatus); }


    // 공격 후딜레이 애니메이션 재생
    // Attack -> Reload는 Animator 창에서 지정됨
    protected void ReloadStatus()
    { SwitchStatusWhenAnimationEnd("Reload", IdleStatus); }


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