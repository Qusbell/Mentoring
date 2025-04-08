using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Actor : MonoBehaviour
{
    [SerializeField] protected int maxHp = 10;  // �ִ� �����
    [SerializeField] protected int nowHp = 10;  // ���� �����

    [SerializeField] protected int attackDamage = 1;   // ���ݷ�
    [SerializeField] protected float attackRate = 1f;  // ���ݰ���
    protected float nextAttackTime = 0f;  // ���� ��, �󸶳� �ð��� �������� ����

    // MeleeAttack
    [SerializeField] protected float attackRange = 10f;    // ���� �Ÿ�
    [SerializeField] protected float attackAngle = 180f;  // ���� ����

    // ���� ��� �±� (�ش� �±׸� ���� ������Ʈ�� ����)
    [SerializeField] protected string targetTag;

    protected Vector3 moveVec;  // �̵��� ����
    [SerializeField] protected float moveSpeed;  // �̵� �ӵ�




    // ����
    public virtual void Attack()
    { Debug.Log("virtual Attack()�� �����"); }


    // �ǰ�
    public virtual void TakeDamage(int damage)
    {
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }

        // ü���� 0 ���Ϸ� �������� ó��
        if (nowHp <= 0)
        { Die(); }
    }

    // ��� ó��
    protected virtual void Die()
    {
        // �ڽ� Ŭ�������� ������ ����
    }

    // �̵� �޼���
    public virtual void Move()
    { Debug.Log("virtual Move()�� �����"); }
}
