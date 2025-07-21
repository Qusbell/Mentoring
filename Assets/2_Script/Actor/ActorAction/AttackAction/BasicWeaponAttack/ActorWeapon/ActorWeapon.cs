using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

abstract public class ActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    protected Collider weaponCollider = null;


    protected virtual void Awake()
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

    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    protected Dictionary<GameObject, int> hitTargets = null;


    // attackAction으로부터 가져올 정보
    protected string targetTag = "";
    protected int attackDamage = 0;
    protected int maxHitCount = 1; // 최대 히트 횟수
    protected GameObject owner = null; // 해당 무기를 소유하고 있는 개체
    protected float knockBack = 0;

    public virtual void SetWeapon(string p_targetTag, GameObject p_owner)
    {
        targetTag = p_targetTag;
        owner = p_owner;
    }


    // 활성화된 횟수
    private int _activateStack = 0;
    protected int activateStack
    {
        get { return _activateStack; }
        set
        {
            if (value < 0) { value = 0; }
            _activateStack = value;
        }
    }

    public virtual void UseWeapon(int p_attackDamage, int p_maxHitCount, float p_knockBackPower = 0)
    {
        activateStack++;
        isActivate = true;
        // attackWeaponType = type;

        attackDamage = p_attackDamage;
        maxHitCount = p_maxHitCount;
        knockBack = p_knockBackPower;
    }

    public virtual void NotUseWeapon()
    {
        activateStack--;
        if (activateStack == 0)
        { isActivate = false; }
    }


    // ===== 트리거 엔터 기반 판정 =====
    protected virtual void OnTriggerEnter(Collider other)
    {
        DamageReaction damageReaction = other.GetComponent<DamageReaction>();

        if (other.CompareTag(targetTag) && // 공격 대상 태그 일치 확인
            damageReaction != null &&      // 데미지 입히기 가능 확인
            hitTargets != null)
        { WeaponCollisionEnterAction(other, damageReaction); }
    }

    protected abstract void WeaponCollisionEnterAction(Collider other, DamageReaction damageReaction);
}