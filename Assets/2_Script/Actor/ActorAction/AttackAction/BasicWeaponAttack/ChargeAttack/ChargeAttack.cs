using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttack : BasicWeaponAttack
{
    protected Rigidbody rigid;
    [SerializeField] protected int chargeSpeed = 20;

    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody>();
    }

    protected override void DoAttack()
    {
        base.DoAttack();
        rigid.AddForce(this.transform.forward * chargeSpeed, ForceMode.Impulse);
    }
}
