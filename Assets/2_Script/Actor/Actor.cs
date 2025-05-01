using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



// RequireComponent : attack / move
[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(DamageReaction))]
abstract public class Actor : MonoBehaviour
{
    // ������Ʈ�� ���� ����ȿ��
    protected Rigidbody rigid;

    // ���ʿ��� ȸ�� ���� ����
    protected virtual void FreezeVelocity()
    { rigid.angularVelocity = Vector3.zero; }


    // �̵�
    protected MoveAction moveAction;
    // ����
    protected AttackAction attackAction;
    // �ǰ�
    protected DamageReaction damageReaction;

    // �ִϸ��̼�
    protected ActorAnimation animatior;


    // ���� �ʱ�ȭ
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animatior = GetComponent<ActorAnimation>();

        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        damageReaction = GetComponent<DamageReaction>();
    }


    // ���������� �Բ� ������Ʈ (0.02s)
    protected virtual void FixedUpdate()
    { FreezeVelocity(); }
}