using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// <- ���Ͷ�� �������� ���۵�
public class FireAction : AttackAction
{
    protected override void Awake()
    {
        base.Awake();
        doAttack = DoAttack;
    }

    private void Start()
    {
        target = TargetManager.instance.Targeting();
    }

    // �߻�ü
    [SerializeField] protected GameObject projectile;

    // �ӽ� Ÿ��
    public Transform target; // <- �ݵ�� player���� �����ϰ� ��


    protected void DoAttack()
    {
        // ����ü �����ϱ�
        GameObject instantProjectile = Instantiate(projectile, this.transform.position, this.transform.rotation); // <- �߻� position ����

        // ����ü �̵� ��� ������
        ProjectileMove moveAction = instantProjectile.GetComponent<ProjectileMove>();

        // �߻� ���� ����
        if (moveAction != null)
        { moveAction.SetTarget(target.position); }
        else { Debug.Log("FireAction : �߸��� Projectile ��ϵ� : " + gameObject.name); }
    }

}
