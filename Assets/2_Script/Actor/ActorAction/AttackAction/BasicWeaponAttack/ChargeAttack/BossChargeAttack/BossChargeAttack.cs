using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChargeAttack : ChargeAttack
{
    protected override void BeforeAttack()
    {
        base.BeforeAttack();
        Timer.Instance.StartRepeatTimer(this, "_Rotate", weaponBeforeDelay * 0.8f,
            () => { WarningPlaneSetter.RotateWarning(this, ref warningPlane, transform.position, transform.forward); });
    }


}
