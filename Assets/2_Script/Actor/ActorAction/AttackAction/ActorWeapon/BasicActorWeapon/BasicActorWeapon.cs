using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 무기 오브젝트에 장착 (자동 포함)
public class BasicActorWeapon : ActorWeapon
{
    private void Start()
    {
        // 이미 다른 BasicActorWeapon이 존재하는 경우 : 스스로 제거
        BasicActorWeapon[] weapons = GetComponents<BasicActorWeapon>();
        foreach (BasicActorWeapon weapon in weapons)
        { if (weapon != this) { Destroy(this); } }
    }


    protected override void WeaponCollisionEnterAction(DamageReaction damageReaction)
    {
        // Debug.Log("실행");
        base.WeaponCollisionEnterAction(damageReaction);
        InstantHitEffect(damageReaction.transform.position);
    }




    // ===== 콜라이더 기반 실제 데미지 판정 =====


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