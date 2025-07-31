using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActorAction : MonoBehaviour
{
    protected Actor thisActor;

    protected virtual void Awake()
    {
        thisActor = GetComponent<Actor>();
    }


}
