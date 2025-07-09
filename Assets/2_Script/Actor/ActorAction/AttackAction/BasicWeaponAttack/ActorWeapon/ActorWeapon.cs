using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// 무기 오브젝트에 장착 (자동 포함)
public class ActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    private Collider weaponCollider = null;

    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    private Dictionary<GameObject, int> hitTargets = null;

    // 무기 콜라이더의 활성/비활성 여부
    public bool isActivate
    {
        set
        {
            weaponCollider.enabled = value;

            // 활성화 시
            if (value)
            { hitTargets = new Dictionary<GameObject, int>(); }

            // 비활성화 시
            else
            { hitTargets = null; }
        }
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
                if (hitTargets != null)
                {
                    int hitCount = 0;
                    hitTargets.TryGetValue(other.gameObject, out hitCount);

                    // 최대 히트 횟수 확인 및 처리
                    if (hitCount < maxHitCount)
                    {
                        hitTargets[other.gameObject] = hitCount + 1; // hitCount += 1
                        damageReaction.TakeDamage(attackDamage, this.gameObject); // 데미지 적용
                    }
                    // else: 최대 히트 횟수 도달 시 추가 동작 없음(무시)
                }
            }
        }
    }


    // ===== 무기 설정 =====

    // attackAction으로부터 가져올 정보
    private string targetTag = "";
    private int attackDamage = 0;
    private int maxHitCount = 1; // 최대 히트 횟수

    public void SetWeapon(string p_targetTag, int p_attackDamage, int p_maxHitCount = 1)
    {
        targetTag = p_targetTag;
        attackDamage = p_attackDamage;
        maxHitCount = p_maxHitCount;
    }
}