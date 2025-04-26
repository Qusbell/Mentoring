using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Actor : MonoBehaviour
{
    // ������Ʈ�� ���� ����ȿ��
    protected Rigidbody rigid;

    // ���ʿ��� ȸ�� ���� ����
    protected virtual void FreezeVelocity()
    { rigid.angularVelocity = Vector3.zero; }

    // ���� �� �ʱ�ȭ
    protected virtual void Awake()
    {
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();
        // null �ʱ�ȭ ���
        if (rigid == null)
        {
            Debug.LogError("Rigidbody ������Ʈ ����!", gameObject);
            enabled = false; // ���� ���
        }
    }

    // ���������� �Բ� ������Ʈ (0.02s)
    protected virtual void FixedUpdate()
    {
        FreezeVelocity();
    }


    //==================================================
    // �̵�/ȸ�� �޼���
    //==================================================
    #region �̵�/ȸ�� �޼���

    // �̵��� ����
    protected Vector3 moveVec;
    // �̵� �ӵ�
    [SerializeField] protected float moveSpeed;


    // �̵� �޼���
    // Update()���� ����
    public virtual void Move()
    {
        if (moveVec != Vector3.zero)
        {
            // ������ġ += ���� * �̵� ���� * �̵� ���� ����
            rigid.MovePosition(rigid.position
            + moveVec * moveSpeed * Time.deltaTime);
        }
    }

    // ȸ��
    // ���� ������ �ٶ�
    public virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }
    #endregion


    //==================================================
    // ���� �޼���
    //==================================================
    #region ���� �޼���

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



    // ����
    public virtual void Attack()
    { Debug.Log("virtual Attack()�� �����"); }

    #endregion


    //==================================================
    // �ǰ�/��� �޼���
    //==================================================
    #region �ǰ�/��� �޼���

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



    // �ǰ� ����
    protected virtual void DamageReaction()
    {

    }


    // ��� ó��
    protected virtual void Die()
    {

    }
    #endregion

}