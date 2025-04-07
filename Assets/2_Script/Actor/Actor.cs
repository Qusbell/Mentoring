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
    [SerializeField] protected string targetTag = "Enemy";

    protected Vector3 moveVec;  // �̵��� ����
    [SerializeField] protected float moveSpeed;  // �̵� �ӵ�


    // ����
    public virtual void Attack()
    {
        // ���� ����
        MeleeBasicAttack();
    }


    void MeleeBasicAttack()
    {
        Debug.Log("���� ����");

        // �±� ��� �˻��� ���� ���� ��� ���� ������Ʈ �� targetTag�� ���� �͵��� ã��
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        // ������ ��� Ÿ�� ���� ������Ʈ�� ���� �ݺ�
        foreach (GameObject target in targets)
        {
            // �ڽŰ� Ÿ�� ������ �Ÿ� ���
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // �Ÿ��� ���� �ݰ� ���� �ִ��� Ȯ��
            if (distanceToTarget <= attackRange)
            {
                // �ڽſ��� Ÿ�ٱ����� ���� ���� ���
                Vector3 directionToTarget = target.transform.position - transform.position;
                directionToTarget.y = 0;  // Y�� ���� 0���� �����Ͽ� ����鿡���� ���� ��� (���� ���� ����)

                // �ڽ��� ���� ���Ϳ� Ÿ�� ���� ���� ������ ���� ���
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                // ���� ������ ���� ������ ���ݺ��� ������ Ȯ��
                if (angleToTarget <= attackAngle / 2)
                {
                    // Actor ������Ʈ�� �ִ��� Ȯ���ϰ� ������ ó��
                    Actor targetActor = target.GetComponent<Actor>();
                    if (targetActor != null)
                    {
                        targetActor.TakeDamage(attackDamage);
                        Debug.Log("���� ����");
                        // ����� �ð�ȭ: ������ ������ Ÿ�ٱ��� ������ �� �׸���
                        Debug.DrawLine(transform.position, target.transform.position, Color.red, 1f);
                    }
                }
            }
        }
    }


    // �ǰ�
    public virtual void TakeDamage(int damage)
    {
        // �ڽ� Ŭ�������� ������ ����
        nowHp -= damage;

        // ü���� 0 ���Ϸ� �������� ó��
        if (nowHp <= 0)
        {
            Die();
        }
    }

    // ��� ó��
    protected virtual void Die()
    {
        // �ڽ� Ŭ�������� ������ ����
    }

    // �̵� �޼��� (�߻� �޼���� �ڽ� Ŭ�������� �ݵ�� �����ؾ� ��)
    public abstract void Move();
}
