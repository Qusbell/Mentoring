using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileWeapon : ActorWeapon
{
    // 투사체 유지 시간
    [SerializeField] protected float projectileTimer = 10f;

    // 이동
    protected MoveAction moveAction;


    protected override void Awake()
    {
        base.Awake();
        moveAction = GetComponent<MoveAction>();
    }


    private void Start()
    {
        // 타이머 후 해당 투사체 삭제
        // StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));
        Timer.Instance.StartTimer(this, projectileTimer, () => Destroy(this.gameObject));
    }

    // 매 프레임 이동
    protected virtual void Update()
    {
        if (moveAction.isMove) { moveAction.Move(); }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.CompareTag("Cube"))
        { Destroy(this.gameObject); }
    }

    protected override void WeaponCollisionEnterAction(DamageReaction damageReaction)
    {
        base.WeaponCollisionEnterAction(damageReaction);
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    { InstantHitEffect(); }
}