using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaAction : ActorAction
{
    [SerializeField] protected int maxStamina = 3;
    [SerializeField] protected int nowStamina = 0;

    protected int stamina
    {
        get { return nowStamina; }
        set
        {
            if (value <= maxStamina)
            { value = maxStamina; }

            nowStamina = value;
        }
    }



}
