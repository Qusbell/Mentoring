using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamageReaction : DamageReaction
{
    public override void TakeDamage(int damage, Actor enemy, float knockBackPower = 0, float knockBackHeight = 0)
    {
        Monster monster = GetComponent<Monster>();
        if (monster != null)
        {
            Transform tempTrans = monster.target;
            monster.target = enemy.transform;
        }

        base.TakeDamage(damage, enemy, knockBackPower, knockBackHeight);
    }
}
