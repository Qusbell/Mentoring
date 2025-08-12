using System;
using Unity.VisualScripting;
using UnityEngine;

public class Boss : Monster
{
    // 공격 횟수 카운트
    // 3 2 1 -> 다음 공격
    protected MaxNowInt normalAttackCount = new MaxNowInt(2);
    protected MaxNowInt chargeAttackCount = new MaxNowInt(1);
    protected MaxNowInt beforeJumpCycleCount = new MaxNowInt(0);

    // 공격 플래그
    private bool attackPlag = true;

    // 돌진 중 충돌 판정
    protected bool hitWhenChargePlag = false;

    // wave를 실행할 위치
    [SerializeField] protected Transform wavePos;


    protected override void Awake()
    {
        base.Awake();
        isBoss = true;

        // --- 스턴 이벤트 등록 ---
        nowAttackKey = AttackName.Monster_BossChargeAttack;
        BossChargeAttack bossChargeAttack = attackAction as BossChargeAttack;
        if (bossChargeAttack != null)
        {
            System.Action action =
                () => {
                    animator.PlayAnimation("DoHit");
                    animator.PlayAnimation("IsHit", true);
                    hitWhenChargePlag = true;
                    SwitchStatus(HitStatus);
                };
            bossChargeAttack.stunEvent.Add(action);
        }

        // --- 점프 애니메이션 ---
        foot.whenJumpEvent.Add(() => { animator.PlayAnimation("IsJump", true); });
        foot.whenGroundEvent.Add(() => { animator.PlayAnimation("IsJump", false); });

        // --- 시작 공격 선택 ---
        nowAttackKey = AttackName.Monster_BossChargeAttack;
    }


    // protected override void LateUpdate()
    // {
    //     base.LateUpdate();
    //     // animator.PlayAnimation("IsJump", !foot.isRand);
    // }


    protected override void SwitchStatus(Action nextStatus)
    {
        base.SwitchStatus(nextStatus);
        attackPlag = true;
        hitWhenChargePlag = false;
        animator.PlayAnimation("IsHit", false);
    }


    protected override void AttackAnimationStatus()
    {
        switch (nowAttackKey)
        {
            case AttackName.Monster_BossNormalAttack:
                if (attackPlag)
                {
                    attackPlag = false;
                    normalAttackCount -= 1;
                }
                PlayTriggerAnimationOnce("DoNomalAttack_1");
                SwitchStatusWhenAnimationEnd("Nomal_Attack_1", NextSelectStatus);
                break;

            case AttackName.Monster_BossChargeAttack:
                if (attackPlag)
                {
                    attackPlag = false;
                    chargeAttackCount -= 1;
                }
                PlayTriggerAnimationOnce("DoChargeAttack");
                if (!hitWhenChargePlag)
                { SwitchStatusWhenAnimationEnd("Charge_Attack", NextSelectStatus); }
                // (BossChargeAttack에서 애니메이션 직접 제어 중)
                break;

                // <- dropAttack
        }
    }


    // 현재 구조
    // (돌진 1번 -> 평타 2번) * 사이클 2번 -> 점프 -> 반복
    protected void NextSelectStatus()
    {
        System.Action nextAction = IdleStatus;

        Debug.Log($"돌진:{chargeAttackCount.now} / 평타:{normalAttackCount.now} / 사이클: {beforeJumpCycleCount.now}");

        if (0 < chargeAttackCount)
        {
            nowAttackKey = AttackName.Monster_BossChargeAttack;
        }
        else if (0 < normalAttackCount)
        {
            nowAttackKey = AttackName.Monster_BossNormalAttack;
        }
        else if (0 < beforeJumpCycleCount)
        {
            nowAttackKey = AttackName.Monster_BossChargeAttack;
            chargeAttackCount.Reset();
            normalAttackCount.Reset();
            beforeJumpCycleCount -= 1;
        }
        else if (0 == beforeJumpCycleCount)
        {
            nowAttackKey = AttackName.Monster_BossChargeAttack;
            chargeAttackCount.Reset();
            normalAttackCount.Reset();
            beforeJumpCycleCount.Reset();
            nextAction = JumpStatus;
        }

        SwitchStatus(nextAction);
    }




    // <- Boss가 ChargeAttack 중 Cube를 만났을 경우, 이 상태로 전환
    protected void StunStatus()
    {
    }



    protected bool tempPlag = true;
    // 점프 준비
    protected void PrepareJumpStatus()
    {
        if (tempPlag)
        {
            tempPlag = false;
            Timer.Instance.StartTimer(this, "_점프준비", 1f, () => { tempPlag = true; SwitchStatus(JumpStatus); });
        }
    }

    // 점프 중 업데이트
    protected void JumpStatus()
    {
        if (this.transform.position.y <= 80f)
        {
            rigid.useGravity = false;
            rigid.velocity = Vector3.up * 40f;
        }
        else
        {
            rigid.useGravity = true;
            transform.position =
                new Vector3(wavePos.position.x,
                transform.position.y,
                wavePos.position.z);
            SwitchStatus(FallStatus);
        }    
    }


    protected void FallStatus()
    {
        if (tempPlag)
        {
            tempPlag = false;
            foot.whenGroundEvent.Add(() => { tempPlag = true; SwitchStatus(WaveStatus); }, 1, true);
        }
    }


    protected void WaveStatus()
    {

    }    



}
