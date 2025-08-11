using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChargeAttack : ChargeAttack
{
    protected override void BeforeAttack()
    {
        // --- 경고발판 생성 ---
        base.BeforeAttack();

        // --- 애니메이션 실행 ---
        thisActor.animator.PlayAnimation("IsChargeAttack", true);

        // --- 공격 활성화 ---
        // UseWeapon();

        // --- 플레이어를 향해 회전 ---
        Timer.Instance.StartRepeatTimer(this, "_RotateWarning", weaponBeforeDelay * 0.8f,
            () => {
                thisActor.moveAction.Turn();
                WarningPlaneSetter.RotateWarning(this, ref warningPlane, transform.position, transform.forward);
            });
    }


    protected override void NotUseWeapon()
    {
        base.NotUseWeapon();
        thisActor.animator.PlayAnimation("IsChargeAttack", false);
    }

    protected override void EndChargeWhenCube()
    {
        base.EndChargeWhenCube();
        // <- 기절 이벤트
    }

}
