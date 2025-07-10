using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingAction : ActorAction
{

    private void Update()
    {
        if(this.transform.position.y < -40)
        {
            Destroy(this.gameObject);
            Debug.Log("삭제됨");
        }
    }

}
