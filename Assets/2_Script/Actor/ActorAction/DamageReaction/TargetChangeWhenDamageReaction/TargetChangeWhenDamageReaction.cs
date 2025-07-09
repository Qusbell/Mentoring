using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(DamageReaction))]
[RequireComponent(typeof(Monster))]
public class TargetChangeWhenDamageReaction : MonoBehaviour
{
    private DamageReaction damageReaction;

    private void Start()
    { damageReaction = GetComponent<DamageReaction>(); }



}
