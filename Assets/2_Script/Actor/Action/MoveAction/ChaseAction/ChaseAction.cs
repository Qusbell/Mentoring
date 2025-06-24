using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class ChaseAction : MoveAction
{
    // ----- ai �κ� -----

    // �׺���̼� AI
    protected NavMeshAgent nav;

    // ������ ���
    protected Transform target;

    // ����
    protected override void Awake()
    {
        base.Awake();
        // �׺���̼� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
        // �̵��ӵ� ����
        nav.speed = moveSpeed;

        // ��ġ, ȸ�� �ڵ� ������Ʈ ��Ȱ��
        nav.updatePosition = false;
        nav.updateRotation = false;
    }

    protected virtual void Start()
    { SetTarget(TargetManager.instance.Targeting()); }


    // Ÿ�� ����
    public void SetTarget(Transform p_target)
    { target = p_target; }


    // target�� �Է��� �Ÿ� �̳��� �ִ��� Ȯ��
    public bool InDistance(int distance)
    { return (this.transform.position - target.position).sqrMagnitude <= distance * distance; }


    // ������ ����
    void UpdateDestination()
    {
        if (target != null) { nav.SetDestination(target.position); }
        else
        {
            Debug.Log("target ���� �� : " + gameObject.name);
            nav.SetDestination(this.transform.position);
        }
    }

    // ���� �̵� ����
    void UpdateNextMoveDirection()
    { moveVec = nav.desiredVelocity.normalized; }

    // �׺���̼� ��ġ�� �ڽ� ��ġ ����ȭ
    void UpdateMyPositionOnNav()
    { if (nav.isOnNavMesh) { nav.nextPosition = rigid.position; } }


    // ȸ�� �ӵ�
    [SerializeField] protected float rotationSpeed = 3f;

    // ���� ���� ������ ���� ȸ�� (������)
    protected override void Turn()
    {
        Vector3 direction = moveVec;
        if (moveVec == Vector3.zero)
        {
            direction = target.position - transform.position;
            direction.y = 0;
        }
        else
        { direction = moveVec; }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void Update()
    {
        UpdateDestination();
        UpdateNextMoveDirection(); // ���� ���� ����
        UpdateMyPositionOnNav();   // �׺���̼� ����
        Turn();                    // ȸ��
    }
}