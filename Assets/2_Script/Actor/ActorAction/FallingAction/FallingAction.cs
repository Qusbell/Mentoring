using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingAction : ActorAction
{
    // 떨어졌을 경우의 이벤트
    public MyEvent whenFallingEvent { get; set; } = new MyEvent();

    protected virtual void Update()
    {
        if (this.transform.position.y < -30)
        { whenFallingEvent.CallBack(); }
    }
}
