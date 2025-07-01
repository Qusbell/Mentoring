using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [RequireComponent(typeof(MeleeBasicAttack))]
[RequireComponent(typeof(BasicWeaponAttack))]
public class MinionMonster : Monster
{
    protected override void AttackStatus()
    {
        // 공격 가능하다면
        if (InAttackRange() && attackAction.isCanAttack && !doAttack)
        {
            attackAction.Attack();
            animatior.isAttack = true; // 어택 애니메이션 재생
        }

        if (animatior.CheckAnimationName("Attack"))
        { doAttack = true; }

        else if (doAttack)
        {
            doAttack = false;
            actionStatus = ReloadStatus; // 공격 후딜레이로 이행 }
        }

    }
}
