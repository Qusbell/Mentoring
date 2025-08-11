using System;
using UnityEngine;


public class Boss : Monster
{
    // 어떤 공격 사용할지 설정(디버그용)
    [SerializeField] protected AttackName tempAttackName;

    // 돌진 플래그
    protected bool chargePlag = true;

    // 돌진 중 충돌 판정
    protected bool isHitWhenCharge = false;


    protected override void Awake()
    {
        base.Awake();
        isBoss = true;

        // 스턴 이벤트 등록
        nowAttackKey = AttackName.Monster_BossChargeAttack;
        BossChargeAttack bossChargeAttack = attackAction as BossChargeAttack;
        if (bossChargeAttack != null)
        {
            System.Action action =
                () => {
                    animator.PlayAnimation("DoHit");
                    animator.PlayAnimation("IsHit", true);
                    isHitWhenCharge = true;
                    SwitchStatus(HitStatus);
                };
            bossChargeAttack.stunEvent.Add(action);
        }

        // 임시
        nowAttackKey = tempAttackName;
    }


    protected override void SwitchStatus(Action nextStatus)
    {
        base.SwitchStatus(nextStatus);
        chargePlag = true;
        isHitWhenCharge = false;
        animator.PlayAnimation("IsHit", false);
    }


    protected override void AttackAnimationStatus()
    {
        switch (nowAttackKey)
        {
            case AttackName.Monster_BossNormalAttack:
                PlayTriggerAnimationOnce("DoNomalAttack_1");
                SwitchStatusWhenAnimationEnd("Nomal_Attack_1", IdleStatus);
                break;

            case AttackName.Monster_BossChargeAttack:
                PlayTriggerAnimationOnce("DoChargeAttack");
                if (!isHitWhenCharge)
                { SwitchStatusWhenAnimationEnd("Charge_Attack", IdleStatus); }
                // (BossChargeAttack에서 애니메이션 직접 제어 중)
                break;

                // <- dropAttack
        }
    }

}
