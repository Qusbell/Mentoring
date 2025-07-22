using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class DropActorWeapon : ActorWeapon
{
    // 범위
    protected float attackRange = 5f;

    // 원래 레이어
    protected int originalLayer = -1;

    // 착지판정
    protected FootCollider foot;


    private void Start()
    {
        // 이미 다른 DropActorWeapon이 존재하는 경우 : 스스로 제거
        DropActorWeapon[] weapons = GetComponents<DropActorWeapon>();
        foreach (DropActorWeapon weapon in weapons)
        { if (weapon != this) { Destroy(this); } }

        // 착지판정 콜라이더
        foot = owner.GetComponentInChildren<FootCollider>();
    }


    public override void UseWeapon(int p_attackDamage, int p_maxHitCount, float p_knockBackPower = 0)
    {
        base.UseWeapon(p_attackDamage, p_maxHitCount, p_knockBackPower);
        if (foot != null)
        {
            foot.ground.Add(DropAttack);
            foot.ground.Add(NotUseWeapon);
        }
    }

    public override void NotUseWeapon()
    {
        base.NotUseWeapon();
        foot.ground.Remove(NotUseWeapon);
    }


    protected override void WeaponCollisionEnterAction(DamageReaction damageReaction)
    {
        base.WeaponCollisionEnterAction(damageReaction);
        DropAttack();
    }


    public void SetWeapon(string p_targetTag, float p_attackRange, int p_originalLayer, GameObject p_owner)
    {
        SetWeapon(p_targetTag, p_owner);
        attackRange = p_attackRange;
        originalLayer = p_originalLayer;

        // 오류거리 정정
        if (attackRange <= 0) { Debug.Log($"{this.owner.name} : DropActorWeapon의 attackRange가 0 이하"); attackRange = 1; }
        if (originalLayer <= -1) { Debug.Log($"{this.owner.name} : DropActorWeapon의 originalLayer 이상"); originalLayer = -1; }
    }


    // 일정 범위 이내의 모든 대상들에게 피해 (WeaponCollisionEnterAction와 완전 독자적)
    protected void DropAttack()
    {
        // ----- 레이어 원복 (Drop 충돌 직후 처리) -----
        owner.layer = originalLayer;

        // ----- 콜라이더 탐색 -----
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, attackRange);

        foreach (Collider collider in colliders)
        {
            GameObject target = collider.gameObject;

            // DamageReaction 컴포넌트가 있으면
            DamageReaction reaction = target.GetComponent<DamageReaction>();
            if (reaction != null && reaction.gameObject.layer != originalLayer)
            { reaction.TakeDamage(attackDamage, owner, knockBack); }
        }

        // ----- 착지 이벤트 제거 -----
        foot.ground.Remove(DropAttack);

        // ----- 디버그 -----
        showGizmo = true;
        Timer.Instance.StartTimer(this, "_기즈모", 0.2f, () => showGizmo = false);
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
