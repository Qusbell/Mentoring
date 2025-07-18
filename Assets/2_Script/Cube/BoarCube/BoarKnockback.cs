using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 넉백 시스템 관리
/// 돌진 중 경로상의 대상들에게 넉백 효과 적용
/// </summary>
public class BoarKnockback : MonoBehaviour
{
    #region ===== 내부 변수 =====

    // 메인 컴포넌트 참조
    private BoarCube main;

    // 넉백 처리된 오브젝트 추적 (중복 넉백 방지)
    private HashSet<GameObject> knockbackedObjects = new HashSet<GameObject>();

    #endregion

    #region ===== 초기화 =====

    /// <summary>
    /// BoarCube에서 호출하는 초기화 메서드
    /// </summary>
    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarKnockback 초기화 완료");
    }

    /// <summary>
    /// 넉백 시스템 리셋 (재사용을 위해)
    /// </summary>
    public void Reset()
    {
        knockbackedObjects.Clear();

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarKnockback 리셋 완료");
    }

    #endregion

    #region ===== 공개 메서드 =====

    /// <summary>
    /// 넉백 대상 체크 및 실행 (BoarMovement에서 호출)
    /// </summary>
    public void CheckKnockback()
    {
        if (!main.enableKnockback)
            return;

        PerformKnockbackDetection();
    }

    #endregion

    #region ===== 내부 메서드 - 넉백 감지 =====

    /// <summary>
    /// 넉백 대상 감지 및 처리
    /// </summary>
    private void PerformKnockbackDetection()
    {
        Vector3 direction = GetMovementDirection();
        float actualWidth = GetActualAttackWidth();

        // 큐브 앞면 중심점 계산
        Vector3 frontCenter = transform.position + direction * (actualWidth / 2f);

        // 박스 형태로 판정 영역 설정
        Vector3 boxSize = new Vector3(actualWidth, actualWidth, main.knockbackRadius);
        Quaternion boxRotation = Quaternion.LookRotation(direction);

        // 판정 영역 내의 모든 콜라이더 검색
        Collider[] hitColliders = Physics.OverlapBox(frontCenter, boxSize / 2f, boxRotation);

        ProcessHitColliders(hitColliders, direction);
    }

    /// <summary>
    /// 감지된 콜라이더들 처리
    /// </summary>
    private void ProcessHitColliders(Collider[] hitColliders, Vector3 direction)
    {
        int knockbackCount = 0;

        foreach (Collider hit in hitColliders)
        {
            GameObject hitObject = hit.gameObject;

            // 자기 자신은 제외
            if (hitObject == gameObject)
                continue;

            // 이미 넉백당한 대상은 제외
            if (knockbackedObjects.Contains(hitObject))
                continue;

            // 넉백 대상인지 확인
            if (IsValidKnockbackTarget(hitObject))
            {
                ExecuteKnockback(hitObject, direction);
                knockbackedObjects.Add(hitObject);
                knockbackCount++;

                if (main.showDebugLog)
                    Debug.Log($"[{gameObject.name}] {hitObject.name} 넉백 적용!");
            }
        }

        // 이번 프레임에 넉백한 대상 수 로그
        if (knockbackCount > 0 && main.showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 이번 프레임 넉백 대상: {knockbackCount}개");
        }
    }

    /// <summary>
    /// 넉백 대상 유효성 검사
    /// </summary>
    private bool IsValidKnockbackTarget(GameObject target)
    {
        // 설정된 태그들 중 하나와 일치하는지 확인
        foreach (string tag in main.knockbackTags)
        {
            if (target.CompareTag(tag))
                return true;
        }
        return false;
    }

    #endregion

    #region ===== 내부 메서드 - 넉백 실행 =====

    /// <summary>
    /// 개별 대상에게 넉백 효과 적용
    /// </summary>
    private void ExecuteKnockback(GameObject target, Vector3 direction)
    {
        // 넉백 방향 계산 (약간의 상승 효과 포함)
        Vector3 knockbackDirection = CalculateKnockbackDirection(direction);

        // Rigidbody가 있으면 물리 기반 넉백
        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody != null)
        {
            ApplyPhysicsKnockback(targetRigidbody, knockbackDirection);
        }
        else
        {
            // Rigidbody가 없으면 Transform 기반 넉백
            StartCoroutine(ApplyTransformKnockback(target, knockbackDirection));
        }

        // 게임 시스템에 넉백 이벤트 알림
        NotifyGameSystems(target);
    }

    /// <summary>
    /// 넉백 방향 계산
    /// </summary>
    private Vector3 CalculateKnockbackDirection(Vector3 baseDirection)
    {
        // 기본 방향 + 약간의 상승 효과
        Vector3 knockbackDirection = baseDirection + Vector3.up * 0.3f;
        return knockbackDirection.normalized;
    }

    /// <summary>
    /// 물리 기반 넉백 적용
    /// </summary>
    private void ApplyPhysicsKnockback(Rigidbody targetRigidbody, Vector3 direction)
    {
        // 기존 속도 제거 후 넉백 힘 적용
        targetRigidbody.velocity = Vector3.zero;
        targetRigidbody.AddForce(direction * main.knockbackForce, ForceMode.VelocityChange);

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 물리 넉백 적용: {targetRigidbody.gameObject.name}");
    }

    /// <summary>
    /// Transform 기반 넉백 (코루틴)
    /// </summary>
    private IEnumerator ApplyTransformKnockback(GameObject target, Vector3 direction)
    {
        if (target == null) yield break;

        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + direction * (main.knockbackForce * 0.5f);
        float elapsed = 0f;

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] Transform 넉백 시작: {target.name}");

        // 부드러운 곡선 이동
        while (elapsed < main.knockbackDuration && target != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / main.knockbackDuration;

            // 이즈아웃 곡선 적용 (처음엔 빠르게, 나중엔 천천히)
            float curveProgress = 1f - (1f - progress) * (1f - progress);

            target.transform.position = Vector3.Lerp(startPos, endPos, curveProgress);
            yield return null;
        }

        // 최종 위치 보정
        if (target != null)
        {
            target.transform.position = endPos;

            if (main.showDebugLog)
                Debug.Log($"[{gameObject.name}] Transform 넉백 완료: {target.name}");
        }
    }

    /// <summary>
    /// 게임 시스템에 넉백 이벤트 알림
    /// </summary>
    private void NotifyGameSystems(GameObject target)
    {
        // 몬스터 시스템에 알림
        var monster = target.GetComponent<Monster>();
        if (monster != null)
        {
            // 향후 몬스터 넉백 반응 시스템 추가 가능
            // monster.OnKnockbackReceived();
        }

        // 플레이어 시스템에 알림
        if (target.CompareTag("Player"))
        {
            target.SendMessage("OnKnockbackReceived", SendMessageOptions.DontRequireReceiver);
        }
    }

    #endregion

    #region ===== 유틸리티 메서드 =====

    /// <summary>
    /// 현재 이동 방향 계산
    /// </summary>
    private Vector3 GetMovementDirection()
    {
        Vector3 endPos = transform.position;
        Vector3 startPos = transform.position + main.startPositionOffset;
        return (endPos - startPos).normalized;
    }

    /// <summary>
    /// 실제 공격 폭 계산
    /// </summary>
    private float GetActualAttackWidth()
    {
        // 설정된 값이 있으면 사용
        if (main.attackWidth > 0f)
            return main.attackWidth;

        // 렌더러 크기 기반 계산
        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);
        }

        // 기본값: Transform 스케일 기반
        return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }

    #endregion

    #region ===== 정보 조회 =====

    /// <summary>
    /// 현재까지 넉백당한 대상 수 반환
    /// </summary>
    public int GetKnockbackedCount()
    {
        return knockbackedObjects.Count;
    }

    /// <summary>
    /// 특정 대상이 이미 넉백당했는지 확인
    /// </summary>
    public bool HasBeenKnockbacked(GameObject target)
    {
        return knockbackedObjects.Contains(target);
    }

    #endregion

    #region ===== 디버그 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 넉백 범위 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (main == null || !main.enableKnockback) return;

        Vector3 direction = GetMovementDirection();
        float actualWidth = GetActualAttackWidth();

        // 넉백 판정 영역 표시
        Vector3 frontCenter = transform.position + direction * (actualWidth / 2f);
        Vector3 boxSize = new Vector3(actualWidth, actualWidth, main.knockbackRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색
        Gizmos.matrix = Matrix4x4.TRS(frontCenter, Quaternion.LookRotation(direction), Vector3.one);
        Gizmos.DrawCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;

        // 방향 화살표
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, direction * actualWidth);
    }

    /// <summary>
    /// 선택 시 상세 정보 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (main == null || !main.showDebugLog) return;

        // 넉백당한 대상들 표시
        Gizmos.color = Color.yellow;
        foreach (GameObject knockbackedObj in knockbackedObjects)
        {
            if (knockbackedObj != null)
            {
                Gizmos.DrawWireSphere(knockbackedObj.transform.position, 0.5f);
            }
        }

        // 현재 상태 텍스트 표시
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 3f,
            $"넉백 대상 수: {knockbackedObjects.Count}\n" +
            $"넉백 활성화: {main.enableKnockback}\n" +
            $"넉백 힘: {main.knockbackForce}"
        );
    }
#endif

    #endregion
}