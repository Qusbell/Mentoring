using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// RequireComponent : DoAttack / move
[RequireComponent(typeof(DamageReaction))]
abstract public class Actor : MonoBehaviour
{
    // ������Ʈ�� ���� ����ȿ��
    protected Rigidbody rigid;

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
        // �������� ����
        rigid = GetComponent<Rigidbody>();
        // ����ȸ�� ����
        rigid.freezeRotation = true;

        animatior = GetComponent<ActorAnimation>();
        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        damageReaction = GetComponent<DamageReaction>();
    }
}