using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class DropActorWeapon : BasicActorWeapon
{
    // 범위
    public float attackRange { get; set; } = 5f;

    // 원래 레이어
    public int originalLayer { get; set; } = -1;

    // ===== 낙하 완료 시 (검에 뭔가 닿았을 경우) 발생 =====
    protected override void OnTriggerEnter(Collider other)
    {
        switch(attackWeaponType)
        {
            case AttackWeaponType.BasicAttack:
                base.OnTriggerEnter(other);
                break;

            case AttackWeaponType.DropAttack:
                base.OnTriggerEnter(other);
                DropAttack();
                NotUseWeapon();

                // 디버그
                showGizmo = true;
                Timer.Instance.StartTimer(this, "_기즈모", 0.5f, () => showGizmo = false);
                break;
        }
    }


    public void SetWeapon(string p_targetTag, float p_attackRange, int p_originalLayer)
    {
        targetTag = p_targetTag;
        attackRange = p_attackRange;
        originalLayer = p_originalLayer;
    }


    // 일정 범위 이내의 모든 대상들에게 피해
    protected void DropAttack()
    {
        // 무기 레이어 원복 (Drop 충돌 직후 처리)
        LayerNameChanger.ChangeLayerWithAll(this.gameObject, originalLayer);

        // 콜라이더 탐색
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, attackRange);

        foreach (Collider collider in colliders)
        {
            GameObject target = collider.gameObject;

            // DamageReaction 컴포넌트가 있으면
            DamageReaction reaction = target.GetComponent<DamageReaction>();
            if (reaction != null && reaction.gameObject.layer != originalLayer)
            { reaction.TakeDamage(attackDamage, this.gameObject); }
        }
    }


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
