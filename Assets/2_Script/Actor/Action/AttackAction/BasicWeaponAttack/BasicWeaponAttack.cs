using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeaponAttack : AttackAction
{
    // <- GameObject weapon: 무기로 사용할 게임오브젝트
    // 미리 지정되지 않으면, 디버그 로그 발생

    // <- 지정된 무기에, ActorWeapon 클래스가 없다면 삽입시킴
    // ActorWeapon의 역할
    // 1. 콜라이더를 트리거로 만듬, 기본적으로 비활성화
    // 2. 본 BasicWeaponAttack과 연결됨 (attackDamage를 ActorWeapon에 전송하는 것도 가능)
    // 3. OnTriggerEnter에서, 이벤트로 누구와 부딪쳤는지 전송 (아니면 ApplyDamage만 적용?)

    // BasicWeaponAttack의 메서드
    // 1. 콜라이더 활성화 / 비활성화 메서드
    // 둘 모두 애니메이션에 직접 삽입?
    



    protected override void DoAttack()
    {


    }



}
