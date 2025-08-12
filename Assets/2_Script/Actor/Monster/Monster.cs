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
        { _target = value; }
    }

    // 추격 행동
    protected ChaseAction chaseAction;

    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작

        // --- 낙사 추가 ---
        if (GetComponent<FallingAction>() == null)
        { this.AddComponent<DestroyWhenFallingAction>(); }

        // moveAction에서 chaseAction 분리
        chaseAction = moveAction as ChaseAction;
        if (chaseAction == null) { Debug.Log(this.gameObject.name + " : ChaseAction 아님"); }
    }


    // 보스
    protected bool isBoss = false;

    protected void AddHitEvent()
    {
        // --- hit 등록 ---
        System.Action hitAction = () => { SwitchStatus(HitStatus); };
        damageReaction.whenHit.AddMulti(hitAction);
    }

    protected void AddDieEvent()
    {
        System.Action dieAction = () => { SwitchStatus(DieStatus); };
        damageReaction.whenDie.AddOnce(dieAction);
    }


    protected void Start()
    {
        // ---
        // 시작 시 1회, 타겟 방향 바라봄 (TurnToTargetWhenStart)
        Vector3 targetPos = target.position;
        targetPos.y = this.transform.position.y; // y값을 동일하게 고정
        this.transform.LookAt(targetPos);        // 평면상에서만 타겟을 바라봄
        // ---

        // --- Hit/Die 추가 ---
        if (!isBoss)
        { AddHitEvent(); }
        AddDieEvent();
    }


    // 현재 수행 중인 행동
    protected System.Action actionStatus;

    protected virtual void Update()
    { actionStatus(); }

    protected virtual void FixedUpdate()
    {
        if (moveAction.isMove)
        { moveAction.Move(); }
    }

    protected virtual void LateUpdate()
    { animator.PlayAnimation("IsMove", moveAction.isMove); }



    // (디버그용) 현재 상태 확인
    [SerializeField] private string currentStateName;
    private void UpdateStateName(Action state)
    { currentStateName = state.Method.Name; }


    // 재생 중 애니메이션 확인
    protected bool isAnimationPlaying = false;

    // 상태 변경 / 애니메이션 체크 초기화
    protected virtual void SwitchStatus(Action nextStatus)
    {
        isTriggerAnimationAvailable = true;
        isAnimationPlaying = false;

        // 이동 상태 종료시키기
        IsMoveOff();

        // 다음 상태로 교체
        actionStatus = nextStatus;
        UpdateStateName(actionStatus); // <- 디버그
    }



    // 트리거 애니메이션의 단일 활성화 보장
    protected bool isTriggerAnimationAvailable = true;
    protected void PlayTriggerAnimationOnce(string animationName)
    {
        if (isTriggerAnimationAvailable)
        {
            isTriggerAnimationAvailable = false;
            animator.PlayAnimation(animationName);
        }
    }

    // 애니메이션 종료 시, 다음 상태로 전환
    protected void SwitchStatusWhenAnimationEnd(string animationName, Action nextStatus)
    {
        // 애니메이션 플래그 = true 설정
        if (animator.CheckAnimationName(animationName))
        { isAnimationPlaying = true; }

        // 이후 애니메이션지 종료되면
        else if (isAnimationPlaying)
        { SwitchStatus(nextStatus); }
    }



    // 공격 사거리 계산
    protected bool CheckInAttackRange()
    { return (target.position - this.transform.position).sqrMagnitude <= attackAction.attackRange * attackAction.attackRange; }

    bool isInAttackRange = false;
    bool isFacing = false;
    bool isClear = false;

    // 공격 준비 완료
    protected bool isReadyToAttack
    {
        get
        {
            isInAttackRange = CheckInAttackRange();
            isFacing = chaseAction.IsFacingTarget();

            if (isInAttackRange)
            { isClear = chaseAction.isClearToTargetAsCash(); }
            else
            { isClear = false; }

            return attackAction.isCanAttack &&
                isInAttackRange &&
                isFacing &&
                isClear;
        }
    }


    // ===== 상태 =====

    // 생성 상태
    private void SpawnState()
    {
        SwitchStatusWhenAnimationEnd("Spawn", IdleStatus);

        // if (!animator.CheckAnimationName("Spawn")) // 스폰 애니메이션 종료 시
        // { SwitchStatus(IdleStatus); }
    }


    // 대기 상태
    protected void IdleStatus()
    {
        chaseAction.ReturnToNav(); // <- nav 위로 되돌아오기

        if (isReadyToAttack)
        { SwitchStatus(AttackStatus); }
        //  else if (!attackAction.isCanAttack)
        //  { moveAction.Turn(); }
        else
        { SwitchStatus(MoveStatus); }
    }


    protected void IsMoveOn()
    {
        // 1회만 true로 변경 (임시)
        // 이유: chaseAction.isMove = true 시에 타이머 발생
        if (!moveAction.isMove)
        { moveAction.isMove = true; }
    }

    protected void IsMoveOff()
    {
        if (moveAction.isMove)
        { moveAction.isMove = false; }
    }

    // 이동 상태
    protected void MoveStatus()
    {
        if (isReadyToAttack)
        {
            IsMoveOff();
            SwitchStatus(AttackStatus);
        }
        else
        {
            if (!isInAttackRange || !isClear)
            { IsMoveOn(); }
            else
            { IsMoveOff(); }

            if (!isFacing)
            { moveAction.Turn(); }
        }
    }


    // 공격 상태
    protected virtual void AttackStatus()
    {
        // 공격 가능하다면
        if (isReadyToAttack)
        {
            attackAction.Attack();
            SwitchStatus(AttackAnimationStatus);
        }
        // 아니면 Idle로
        else { SwitchStatus(IdleStatus); }
    }

    // 공격 애니메이션 상태
    protected virtual void AttackAnimationStatus()
    {
        PlayTriggerAnimationOnce("DoAttack");
        SwitchStatusWhenAnimationEnd("Attack", ReloadStatus);
    }


    // 공격 후딜레이 애니메이션 재생
    // Attack -> Reload는 Animator 창에서 지정됨
    protected void ReloadStatus()
    { SwitchStatusWhenAnimationEnd("Reload", IdleStatus); }


    // 피격 시 애니메이션
    protected virtual void HitStatus()
    {
        PlayTriggerAnimationOnce("IsHit");
        SwitchStatusWhenAnimationEnd("Hit", IdleStatus);
    }

    protected void DieStatus()
    {
        PlayTriggerAnimationOnce("IsDie");
    }



}