using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



// 무기 오브젝트에 장착 (자동 포함)
public class ActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    private Collider weaponCollider = null;

    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    private Dictionary<GameObject, int> hitTargets = null;



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



    // ===== 무기 콜라이더 활성화 / 비활성화 =====
    public bool isActivate
    {
        set
        {
            weaponCollider.enabled = value;

            // 활성화 시
            if (value) { hitTargets = new Dictionary<GameObject, int>(); }

            // 비활성화 시
            else { hitTargets = null; }
        }
    }

    // 활성화된 횟수
    private int _activateStack = 0;
    private int activateStack
    {
        get { return _activateStack; }
        set
        {
            if (value < 0) { value = 0; }
            _activateStack = value;
        }
    }

    public void UseWeapon()
    {
        activateStack++;
        isActivate = true;
    }

    public void UseWeapon(int p_attackDamage, int p_maxHitCount = 1)
    {
        activateStack++;
        isActivate = true;

        attackDamage = p_attackDamage;
        maxHitCount = p_maxHitCount;
    }

    public void NotUseWeapon()
    {
        activateStack--;
        if(activateStack == 0)
        { isActivate = false; }
    }
    


    // ===== 무기 설정 (기존 버전) =====

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


    // 디버그 기즈모
    void OnDrawGizmos()
    {
        if (weaponCollider == null)
        { weaponCollider = GetComponent<Collider>(); }

        // 콜라이더가 비활성화되어 있으면 표시X
        if (weaponCollider != null && weaponCollider.enabled)
        {
            Gizmos.color = Color.red;
            // BoxCollider 예시(다른 콜라이더는 각각 맞게 구현)
            if (weaponCollider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            // SphereCollider, CapsuleCollider 등도 필요시 추가
        }
    }
}