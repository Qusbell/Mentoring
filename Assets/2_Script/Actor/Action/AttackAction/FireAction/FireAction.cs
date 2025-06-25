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

    // 발사 위치
    public Transform firePos;

    protected void DoAttack()
    {
        if (projectile != null)
        {
            // 투사체 생성하기
            GameObject instantProjectile = Instantiate(projectile, firePos.position, this.transform.rotation); // <- 발사 position 조절

            // 투사체 이동 방식 가져옴
            // <- 여기 MoveAction을 GetComponent한 다음, as 키워드로 바꿔끼우는 게 좋을 것 같기도 함
            ProjectileMove moveAction = instantProjectile.GetComponent<ProjectileMove>();

            // 발사 방향 지정
            if (moveAction != null)
            { moveAction.SetTarget(target.position); }
            else { Debug.Log("FireAction : 잘못된 Projectile 등록됨 : " + gameObject.name); }
        }
        else { Debug.Log("Projectile 지정되지 않음 : " + gameObject.name); }
    }
}
