using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//==================================================
// ���� �ൿ
//==================================================
[RequireComponent(typeof(Timer))]
abstract public class AttackAction : MonoBehaviour
{
    // ���ݷ�
    [SerializeField] protected int attackDamage = 1;

    // ���� ��Ÿ�
    [field: SerializeField] public float attackRange { get; protected set; } = 3f;

    // ���� ���� (== ���� �ӵ�)
    [SerializeField] public Timer attackRate;

    // ���� ��� �±� (�ش� �±׸� ���� ������Ʈ�� ����)
    [SerializeField] protected string targetTag = "";

    // <- ���� ��� ���̾�


    protected virtual void Awake()
    {
        attackRate = GetComponent<Timer>();

        // Ÿ���±� �˻�
        // �������� ���� ��� : �������� �����
        if (targetTag == "")
        {
            if (gameObject.tag == "Monster") { targetTag = "Player"; }
            else if (gameObject.tag == "Player") { targetTag = "Monster"; }
        }
    }


    // ����
    public virtual void Attack()
    {
        // <- �Ļ� Ŭ�������� ����
    }
}