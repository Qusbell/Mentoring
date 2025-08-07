using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnWhenSandwitchedReaction : SandwitchedReaction
{
    // 끼었을 경우의 피해량
    [SerializeField] protected int sandwichedDamage = 3;

    RespawnAction respawnAction;
    DamageReaction damageReaction;

    protected override void Awake()
    {
        base.Awake();
        respawnAction = GetComponentInParent<RespawnAction>();
        if (respawnAction == null)
        { Debug.LogError($"{actor.gameObject}에 RespawnAction 없음"); }

        damageReaction = GetComponentInParent<DamageReaction>();
        if (damageReaction == null)
        { Debug.LogError($"{actor.gameObject}에 DamageReaction 없음"); }
    }



    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Cube")) { return; }

        damageReaction.TakeDamage(sandwichedDamage, actor); // 데미지
        respawnAction.ReturnToSafePos();                    // 텔포
    }


}
