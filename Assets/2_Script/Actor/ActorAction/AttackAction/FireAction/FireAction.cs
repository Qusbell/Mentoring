using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


// <- 몬스터라는 가정으로 제작됨
public class FireAction : AttackAction
{
    // attackBeforeDelay
    [SerializeField] protected float weaponBeforeDelay = 0.35f;

    // 발사체
    [SerializeField] protected GameObject projectile;

    // 발사 위치
    [SerializeField] protected Transform firePos;


    // 임시 타겟
    private Transform _target; // <- 반드시 player만을 지정하게 됨
    public Transform target
    {
        get
        {
            if(_target == null)
            {
                Monster monster = GetComponent<Monster>();
                _target = monster.target;
            }
            return _target;
        }
    }


    protected override void DoAttack()
    {
        Timer.Instance.StartTimer(this, "_Fire", weaponBeforeDelay, Fire);
    }


    protected virtual void Fire()
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
            { moveAction.SetTargetPos(target.position); }
            else { Debug.Log("FireAction : 잘못된 Projectile 등록됨 : " + gameObject.name); }
        }
        else { Debug.Log("Projectile 지정되지 않음 : " + gameObject.name); }
    }

}
