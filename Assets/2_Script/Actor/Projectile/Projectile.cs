using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ProjectileHitAttack))]
[RequireComponent (typeof(ProjectileMove))]
public class Projectile : Actor
{
    // 매 프레임 이동
    protected virtual void Update()
    {
        if (moveAction.isMove) { moveAction.Move(); }
    }
}
