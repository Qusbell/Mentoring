using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageReaction : DamageReaction
{
    // 무적 시간
    [SerializeField] protected float invincibilityTime = 1f;
    // 무적 시간 중 깜빡이 주기
    [SerializeField] protected float flikerTime = 0.1f;
    protected bool isInvincibility = false;

    // 렌더러
    private List<Renderer> targetRenderers = new List<Renderer>();
    private bool isBlinkOn = true;

    protected void Start()
    {
        whenHitEvent.Add(Invincibility);
        CollectAllRelevantRenderers();
    }

    // === mesh 깜빡임 ===

    // 모든 mesh 수집
    void CollectAllRelevantRenderers()
    {
        targetRenderers.Clear();

        // 1. 자기 자신 포함 자식 전체
        Renderer[] childrenRenderers = GetComponentsInChildren<Renderer>(true);
        targetRenderers.AddRange(childrenRenderers);

        // 2. 부모들 모두(최상위까지)
        Transform current = transform.parent;
        while (current != null)
        {
            Renderer[] parentRenderers = current.GetComponents<Renderer>();
            if (parentRenderers != null)
            { targetRenderers.AddRange(parentRenderers); }
            current = current.parent;
        }
    }

    // 모든 렌더러 활성화/비활성화
    void SetRenderersEnabled(bool enabled)
    {
        foreach (var rend in targetRenderers)
        {
            if (rend != null)
                rend.enabled = enabled;
        }
    }

    // === 무적 ===
    protected void Invincibility()
    {
        Timer.Instance.StartEndlessTimer(this, "_Fliker", flikerTime, () => SetRenderersEnabled(isBlinkOn = !isBlinkOn));

        isInvincibility = true;
        Timer.Instance.StartTimer(this, "_Invincibility", invincibilityTime,
            () =>
            {
                isInvincibility = false;
                Timer.Instance.StopEndlessTimer(this, "_Fliker");
                SetRenderersEnabled(true);
            });
    }

    // === TakeDamage ===
    public override void TakeDamage(int damage, Actor enemy, float knockBackPower = 0, float knockBackHeight = 0)
    {
        if (!isInvincibility)
        { base.TakeDamage(damage, enemy, knockBackPower, knockBackHeight); }
    }
}