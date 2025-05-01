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
    // ���� ����
    [SerializeField] public Timer attackRate { get; protected set; }

    // ���� ��� �±� (�ش� �±׸� ���� ������Ʈ�� ����)
    [SerializeField] protected string targetTag = null;

    // <- ���� ��� ���̾�


    protected virtual void Awake()
    {
        attackRate = GetComponent<Timer>();

        // Ÿ���±� �˻�
        if (targetTag != null) { return; }
        // �������� ���� ��� : �⺻���� �����
        if (gameObject.tag == "Monster") { targetTag = "Player"; }
        else if (gameObject.tag == "Player") { targetTag = "Monster"; }
    }


    // ����
    public virtual void Attack()
    {
        // <- �Ļ� Ŭ�������� ����
    }
}