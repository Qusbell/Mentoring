using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Boss : Monster
{
    // 어떤 공격 사용할지 설정(디버그용)
    [SerializeField] protected AttackName tempAttackName;

    // 돌진 플래그
    protected bool chargePlag = true;


    protected override void Awake()
    {
        base.Awake();
        nowAttackKey = tempAttackName;
        isBoss = true;
    }


    protected override void SwitchStatus(Action nextStatus)
    {
        base.SwitchStatus(nextStatus);
        chargePlag = true;
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
                SwitchStatusWhenAnimationEnd("Charge_Attack", IdleStatus);
                // <- BossChargeAttack에서 애니메이션 직접 제어 중
                break;

                // <- dropAttack
        }
    }




}
