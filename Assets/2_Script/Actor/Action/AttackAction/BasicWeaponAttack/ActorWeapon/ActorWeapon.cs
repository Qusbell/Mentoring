using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// 무기 오브젝트에 장착 (자동 포함)
public class ActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    private Collider weaponCollider = null;

    // 무기 콜라이더의 활성/비활성 여부
    public bool isActivate
    {
        set
        { weaponCollider.enabled = value; }
    }


    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = true; // 트리거 콜라이더 활성화
            isActivate = false;  // 기본적으로 비활성 (공격 시에만 활성)
        }
        else
        { Debug.Log("ActorWeapon에 콜라이더 미존재 : " + this.gameObject.name); }
    }

    
    // ===== 콜라이더 활성화 / 비활성화 =====

    public void UseWeapon()
    { isActivate = true; }

    public void NotUseWeapon()
    { isActivate = false; }



    // ===== 콜라이더 기반 실제 데미지 판정 =====

    private void OnTriggerEnter(Collider other)
    {
        // 대상 태그가 일치하면
        if (other.CompareTag(targetTag))
        {
            // 피해를 입을 수 있다면
            DamageReaction damageReaction = other.GetComponent<DamageReaction>();
            if (damageReaction != null)
            {
                // 데미지 적용
                damageReaction.TakeDamage(attackDamage);
            }
        }
    }


    // ===== 무기 설정 =====

    // attackAction으로부터 가져올 정보
    private string targetTag = "";
    private int attackDamage = 0;

    public void SetWeapon(string p_targetTag, int p_attackDamage)
    {
        targetTag = p_targetTag;
        attackDamage = p_attackDamage;
    }
}