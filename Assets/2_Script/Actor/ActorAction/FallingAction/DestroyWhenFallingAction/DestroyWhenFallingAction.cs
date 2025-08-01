using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenFallingAction : FallingAction
{
    protected override void Awake()
    {
        base.Awake();
        whenFallingEvent.AddOnce(() => { Destroy(this.gameObject); });
    }
}
