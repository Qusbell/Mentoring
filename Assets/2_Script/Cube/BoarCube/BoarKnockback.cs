using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 넉백 시스템 관리
/// </summary>
public class BoarKnockback : MonoBehaviour
{
    private BoarCube main;
    private HashSet<GameObject> knockbackedObjects = new HashSet<GameObject>();

    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;
    }

    public void Reset()
    {
        knockbackedObjects.Clear();
    }

    public void CheckKnockback()
    {
        Vector3 direction = GetDirection();
        float actualWidth = GetActualAttackWidth();

        // 큐브 앞면 중심점 계산
        Vector3 frontCenter = transform.position + direction * (actualWidth / 2f);

        // 박스 형태로 판정
        Vector3 boxSize = new Vector3(actualWidth, actualWidth, main.knockbackRadius);
        Quaternion boxRotation = Quaternion.LookRotation(direction);

        Collider[] hitColliders = Physics.OverlapBox(frontCenter, boxSize / 2f, boxRotation);

        foreach (Collider hit in hitColliders)
        {
            GameObject hitObject = hit.gameObject;

            if (hitObject == gameObject) continue;
            if (knockbackedObjects.Contains(hitObject)) continue;

            if (IsKnockbackTarget(hitObject))
            {
                ExecuteKnockback(hitObject, direction);
                knockbackedObjects.Add(hitObject);

                if (main.showDebugLog)
                    Debug.Log($"[{gameObject.name}] {hitObject.name} 넉백!");
            }
        }
    }

    private bool IsKnockbackTarget(GameObject target)
    {
        foreach (string tag in main.knockbackTags)
        {
            if (target.CompareTag(tag))
                return true;
        }
        return false;
    }

    private void ExecuteKnockback(GameObject target, Vector3 direction)
    {
        Vector3 knockbackDirection = direction + Vector3.up * 0.3f;
        knockbackDirection = knockbackDirection.normalized;

        // Rigidbody 넉백
        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody != null)
        {
            targetRigidbody.velocity = Vector3.zero;
            targetRigidbody.AddForce(knockbackDirection * main.knockbackForce, ForceMode.VelocityChange);
        }
        else
        {
            StartCoroutine(TransformKnockback(target, knockbackDirection));
        }

        // Actor 시스템 알림
        NotifyActorSystem(target);
    }

    private IEnumerator TransformKnockback(GameObject target, Vector3 direction)
    {
        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + direction * (main.knockbackForce * 0.5f);

        float elapsed = 0f;

        while (elapsed < main.knockbackDuration)
        {
            if (target == null) yield break;

            elapsed += Time.deltaTime;
            float progress = elapsed / main.knockbackDuration;
            float curveProgress = 1f - (1f - progress) * (1f - progress);

            target.transform.position = Vector3.Lerp(startPos, endPos, curveProgress);
            yield return null;
        }

        if (target != null)
            target.transform.position = endPos;
    }

    private void NotifyActorSystem(GameObject target)
    {
        var monster = target.GetComponent<Monster>();
        if (monster != null)
        {
            // monster.OnKnockbackReceived();
        }

        if (target.CompareTag("Player"))
        {
            target.SendMessage("OnKnockbackReceived", SendMessageOptions.DontRequireReceiver);
        }
    }

    // ==================== 유틸리티 ====================

    private Vector3 GetDirection()
    {
        Vector3 endPos = transform.position;
        Vector3 startPos = transform.position + main.startPositionOffset;
        return (endPos - startPos).normalized;
    }

    private float GetActualAttackWidth()
    {
        if (main.attackWidth > 0f)
            return main.attackWidth;

        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);

        return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }
}