using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;



//==================================================
// 피격으로 인한 피해 반응 / 사망 시 처리
//==================================================
public class DamageReaction : MonoBehaviour
{
    [SerializeField] protected int maxHp = 10;  // 최대 생명력
    [SerializeField] protected int nowHp = 10;  // 현재 생명력


    // 피격
    public virtual void TakeDamage(int damage)
    {
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }

        Debug.Log("피격: " + gameObject.name);

        // 피격 반응
        // DamageReaction();

        // 체력이 0 이하로 떨어지면 처리
        if (nowHp <= 0)
        { Die(); }
    }


    // 사망 처리
    protected virtual void Die()
    {

    }
}