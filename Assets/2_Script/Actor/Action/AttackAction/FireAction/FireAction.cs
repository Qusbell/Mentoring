using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// <- 몬스터라는 가정으로 제작됨
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

    // 발사체
    [SerializeField] protected GameObject projectile;

    // 임시 타겟
    public Transform target; // <- 반드시 player만을 지정하게 됨


    protected void DoAttack()
    {
        // 투사체 생성하기
        GameObject instantProjectile = Instantiate(projectile, this.transform.position, this.transform.rotation); // <- 발사 position 조절

        // 투사체 이동 방식 가져옴
        ProjectileMove moveAction = instantProjectile.GetComponent<ProjectileMove>();

        // 발사 방향 지정
        if (moveAction != null)
        { moveAction.SetTarget(target.position); }
        else { Debug.Log("FireAction : 잘못된 Projectile 등록됨 : " + gameObject.name); }
    }

}
