using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 점프 공격
public class DropAttack : AttackAction
{
    //
    protected Rigidbody rigid;

    [SerializeField] protected float dropSpeed = 13f;


    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody>();
    }


    protected override void DoAttack()
    {
        rigid.AddForce(Vector3.down * dropSpeed, ForceMode.Impulse);
    }
}
