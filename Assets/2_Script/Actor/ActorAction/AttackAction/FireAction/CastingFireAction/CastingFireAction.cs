using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastingFireAction : FireAction
{
    protected override void Fire()
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
            { moveAction.SetTargetTransform(target); }
            else { Debug.Log("FireAction : 잘못된 Projectile 등록됨 : " + gameObject.name); }

            // 투사체 활성화
            ProjectileWeapon tempProjectile = instantProjectile.GetComponent<ProjectileWeapon>();
            if (tempProjectile != null)
            {
                tempProjectile.SetWeapon(targetTag, this.gameObject);
                tempProjectile.UseWeapon(attackDamage, maxHitCount, knockBackPower);
            }
        }
        else { Debug.Log("Projectile 지정되지 않음 : " + gameObject.name); }
    }

}
