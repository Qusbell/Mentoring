using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//==================================================
// ���� �ൿ
//==================================================
abstract public class AttackAction : MonoBehaviour
{
    // ���ݷ�
    [SerializeField] protected int attackDamage = 1;

    // ���� ��Ÿ�
    [field: SerializeField] public float attackRange { get; protected set; } = 3f;

    // ���� ���� (== ���� �ӵ�)
    [SerializeField] protected float attackRate;

    // ���� ���� ����
    public bool isCanAttack { get; protected set; } = true;
    


    // ���� ��� �±� (�ش� �±׸� ���� ������Ʈ�� ����)
    [SerializeField] protected string targetTag = "";

    // <- ���� ����� ���̾�


    protected virtual void Awake()
    {
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
        if (isCanAttack == true)
        {
            isCanAttack = false;
            StartCoroutine(Timer.StartTimer(attackRate, () => { isCanAttack = true; }));
        }
    }
}