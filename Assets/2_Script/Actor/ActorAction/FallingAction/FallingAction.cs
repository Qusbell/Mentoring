using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingAction : ActorAction
{

    protected virtual void Update()
    {
        if(this.transform.position.y < -40)
        {
            Destroy(this.gameObject);
            // Debug.Log("삭제됨");
        }
    }

}
