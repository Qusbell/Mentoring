using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//==================================================
// 공격 행동
//==================================================
abstract public class AttackAction : MonoBehaviour
{
    // 공격력
    [SerializeField] protected int attackDamage = 1;

    // 공격 사거리
    [field: SerializeField] public float attackRange { get; protected set; } = 3f;

    // 공격 간격 (== 공격 속도)
    [SerializeField] protected float attackRate;

    // 공격 가능 여부
    public bool isCanAttack { get; protected set; } = true;
    


    // 공격 대상 태그 (해당 태그를 가진 오브젝트만 공격)
    [SerializeField] protected string targetTag = "";

    // <- 공격 대상의 레이어


    protected virtual void Awake()
    {
        // 타겟태그 검사
        // 배정되지 않은 경우 : 기초적인 재배정
        if (targetTag == "")
        {
            if (gameObject.tag == "Monster") { targetTag = "Player"; }
            else if (gameObject.tag == "Player") { targetTag = "Monster"; }
        }
    }


    // 공격
    public virtual void Attack()
    {
        if (isCanAttack == true)
        {
            isCanAttack = false;
            StartCoroutine(Timer.StartTimer(attackRate, () => { isCanAttack = true; }));
        }
    }
}