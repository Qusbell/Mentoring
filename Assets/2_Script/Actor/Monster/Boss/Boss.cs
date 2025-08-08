using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Boss : Monster
{
    // 어떤 공격 사용할지 설정(디버그용)
    [SerializeField] protected AttackName tempAttackName;


    protected override void Awake()
    {
        base.Awake();
        nowAttackKey = tempAttackName;
        isBoss = true;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
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
                animator.PlayAnimation("IsChargeAttack", true);
                break;

                // <- dropAttack
        }
    }

}
