using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandwitchedReaction : MonoBehaviour
{
    // 끼었을 경우의 피해량
    [SerializeField] protected int sandwichedDamage = 3;

    DamageReaction damageReaction;
    RespawnAction respawnAction;
    Actor actor;


    private void Awake()
    {
        damageReaction = GetComponentInParent<DamageReaction>();
        respawnAction = GetComponentInParent<RespawnAction>();
        actor = GetComponentInParent<Actor>();

        if (damageReaction == null || actor == null || respawnAction == null)
        { Debug.Log("몬가몬가 잘못됨"); return; }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Cube")) { return; }

        Debug.Log("충돌 발생");

        // 데미지
        damageReaction.TakeDamage(sandwichedDamage, actor);

        // 텔포
        respawnAction.ReturnToSafePos();
    }


}
