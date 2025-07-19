using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackWeaponType
{
    BasicAttack,
    DropAttack
}



// 무기 오브젝트에 장착 (자동 포함)
public class BasicActorWeapon : MonoBehaviour
{
    // 무기의 콜라이더
    private Collider weaponCollider = null;

    // 공격의 판정 횟수 체크
    // 공격 대상(key) / 히트 횟수(value)
    private Dictionary<GameObject, int> hitTargets = null;

    protected AttackWeaponType attackWeaponType;


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

    public virtual void UseWeapon(int p_attackDamage, int p_maxHitCount, AttackWeaponType type)
    {
        activateStack++;
        isActivate = true;
        attackWeaponType = type;

        attackDamage = p_attackDamage;
        maxHitCount = p_maxHitCount;
    }

    public virtual void NotUseWeapon()
    {
        activateStack--;
        if(activateStack == 0)
        { isActivate = false; }
    }
    

    // ===== 무기 설정 (기존 버전) =====

    // attackAction으로부터 가져올 정보
    protected string targetTag = "";
    protected int attackDamage = 0;
    protected int maxHitCount = 1; // 최대 히트 횟수

    public virtual void SetWeapon(string p_targetTag)
    { targetTag = p_targetTag; }


    // ===== 콜라이더 기반 실제 데미지 판정 =====

    protected virtual void OnTriggerEnter(Collider other)
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



    // ===== 디버그 기즈모 =====
    void OnDrawGizmos()
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<Collider>();
        }

        // 콜라이더가 비활성화되어 있으면 표시 X
        if (weaponCollider != null && weaponCollider.enabled)
        {
            Gizmos.color = Color.red;

            // BoxCollider 렌더링
            if (weaponCollider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }

            // CapsuleCollider 렌더링
            else if (weaponCollider is CapsuleCollider capsule)
            {
                DrawWireCapsule(capsule);
            }
        }
    }

    // CapsuleCollider를 Gizmos로 그려주는 유틸리티 함수
    private void DrawWireCapsule(CapsuleCollider capsule)
    {
        // 캡슐의 매트릭스를 적용
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = capsule.transform.localToWorldMatrix;

        Vector3 center = capsule.center;
        float radius = capsule.radius;
        float height = capsule.height;
        int direction = capsule.direction; // 0=X, 1=Y, 2=Z

        // 캡슐의 시각적 길이 (구면 상하 제외한 부분)
        float sphereHeight = radius * 2f;
        float straightHeight = Mathf.Max(0f, height - sphereHeight);

        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        // 방향 보정
        switch (direction)
        {
            case 0: // X-axis
                up = capsule.transform.right;
                forward = capsule.transform.forward;
                right = capsule.transform.up;
                break;
            case 1: // Y-axis (기본)
                up = capsule.transform.up;
                forward = capsule.transform.forward;
                right = capsule.transform.right;
                break;
            case 2: // Z-axis
                up = capsule.transform.forward;
                forward = capsule.transform.up;
                right = capsule.transform.right;
                break;
        }

        Vector3 topSphere = center + (up * (straightHeight / 2f));
        Vector3 bottomSphere = center - (up * (straightHeight / 2f));

        // 구 두 개 그리기
        Gizmos.DrawWireSphere(topSphere, radius);
        Gizmos.DrawWireSphere(bottomSphere, radius);

        // 원기둥 라인 그리기
        Gizmos.DrawLine(bottomSphere + right * radius, topSphere + right * radius);
        Gizmos.DrawLine(bottomSphere - right * radius, topSphere - right * radius);
        Gizmos.DrawLine(bottomSphere + forward * radius, topSphere + forward * radius);
        Gizmos.DrawLine(bottomSphere - forward * radius, topSphere - forward * radius);

        Gizmos.matrix = oldMatrix;
    }


}