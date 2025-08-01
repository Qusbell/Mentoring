using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RespawnAction))]
public class RespawnWhenFallingAciton : FallingAction
{
    protected override void Awake()
    {
        base.Awake();
        RespawnAction respawnAction = GetComponent<RespawnAction>();
        whenFallingEvent.AddMulti(respawnAction.ReturnToSafePos);
    }


}
