using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class DropActorWeapon : ActorWeapon
{
    protected int splashDamage = 1;   // 범위 공격 피해
    protected float attackRange = 5f; // 범위
    protected int originalLayer = -1; // 원래 레이어

    protected FootCollider foot;  // 착지판정


    private void Start()
    {
        // 이미 다른 DropActorWeapon이 존재하는 경우 : 스스로 제거
        DropActorWeapon[] weapons = GetComponents<DropActorWeapon>();
        foreach (DropActorWeapon weapon in weapons)
        { if (weapon != this) { Destroy(this); } }

        // 착지판정 콜라이더
        foot = owner.GetComponentInChildren<FootCollider>();
    }


    public override void UseWeapon(
        int p_attackDamage,
        int p_maxHitCount, float p_knockBackPower = 0, float p_knockBackHeight = 0,
        GameObject p_hitEffect = null,
        float p_effectDestoryTime = 1f)
    {
        base.UseWeapon(p_attackDamage, p_maxHitCount, p_knockBackPower, p_knockBackHeight, p_hitEffect, p_effectDestoryTime);
        if (foot != null)
        {
            foot.ground.Add(DropAttack);   // 먼저 드랍어택
            foot.ground.Add(NotUseWeapon); // 그 후 무기 사용 종료
        }
    }


    public override void NotUseWeapon()
    {
        base.NotUseWeapon();
        this.owner.rigid.velocity = Vector3.zero;

        foot.ground.Remove(DropAttack);
        foot.ground.Remove(NotUseWeapon);
    }


    public void SetWeapon(string p_targetTag, float p_attackRange, int p_originalLayer, Actor p_owner, int p_splashDamage = 1) // <- splashDamage: 야매
    {
        SetWeapon(p_targetTag, p_owner);
        attackRange = p_attackRange;
        originalLayer = p_originalLayer;
        splashDamage = p_splashDamage;

        // 오류 정정
        if (attackRange <= 0) { Debug.Log($"{this.owner.name} : DropActorWeapon의 attackRange가 0 이하"); attackRange = 1; }
        if (originalLayer <= -1) { Debug.Log($"{this.owner.name} : DropActorWeapon의 originalLayer 이상"); originalLayer = -1; }
        if (splashDamage <= 0) { Debug.Log($"{this.owner.name} : DropActorWeapon의 splashDamage가 0 이하"); splashDamage = 0; }
    }


    // 일정 범위 이내의 모든 대상들에게 피해 (WeaponCollisionEnterAction와 완전 독자적)
    protected void DropAttack()
    {
        // ----- 레이어 원복 (Drop 충돌 직후 처리) -----
        owner.gameObject.layer = originalLayer;

        // ----- 콜라이더 탐색 -----
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, attackRange);

        foreach (Collider collider in colliders)
        {
            GameObject target = collider.gameObject;

            // DamageReaction 컴포넌트가 있으면
            DamageReaction reaction = target.GetComponent<DamageReaction>();
            if (reaction != null && reaction.gameObject.layer != originalLayer)
            { reaction.TakeDamage(splashDamage, owner, knockBackPower, knockBackHeight); }
        }

        // ----- 디버그 -----
        showGizmo = true;
        Timer.Instance.StartTimer(this, "_기즈모", 0.2f, () => showGizmo = false);

        // ----- 이펙트 -----
        InstantHitEffect();
    }


    // ===== 디버그 기즈모 =====
    private bool showGizmo = false;
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
