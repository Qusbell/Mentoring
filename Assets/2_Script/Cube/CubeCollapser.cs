using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 붕괴 큐브 컴포넌트
/// 시간 기반 또는 플레이어 근접에 의한 붕괴 기능 제공
/// </summary>
public class CubeCollapser : MonoBehaviour
{
    [Header("붕괴 설정")]
    [Tooltip("붕괴하는 큐브인지 여부")]
    public bool isCollapsible = true;

    [Tooltip("붕괴 속도 (초당 유닛)")]
    public float collapseSpeed = 5f;

    [Tooltip("붕괴 전 대기 시간 (초)")]
    public float warningDelay = 1f;

    [Tooltip("붕괴 트리거 유형")]
    public TriggerType triggerType = TriggerType.PlayerProximity;

    // 트리거 타입 정의
    public enum TriggerType
    {
        Time,            // 시간 기반 (일정 시간 후 붕괴)
        PlayerProximity  // 플레이어 근접
    }

    [Tooltip("플레이어 근접 트리거의 경우 거리 설정")]
    public float triggerDistance = 2f;

    [Tooltip("플레이어의 태그")]
    public string playerTag = "Player";

    [Tooltip("붕괴 후 오브젝트 비활성화까지 시간")]
    public float destroyDelay = 2f;

    // 내부 변수
    private bool isCollapsing = false;
    private bool hasCollapsed = false;
    private Transform playerTransform;

    // 시작 시 초기화
    void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 시간 트리거인 경우 자동으로 붕괴 시작
        if (triggerType == TriggerType.Time && isCollapsible)
        {
            StartCoroutine(StartCollapseSequence());
        }
    }

    // 매 프레임 실행
    void Update()
    {
        // 이미 붕괴 중이거나 붕괴가 완료된 경우 무시
        if (isCollapsing || hasCollapsed || !isCollapsible) return;

        // 플레이어 근접 트리거 확인
        if (triggerType == TriggerType.PlayerProximity && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= triggerDistance)
            {
                StartCoroutine(StartCollapseSequence());
            }
        }

        // 붕괴 중인 경우 아래로 이동
        if (isCollapsing && !hasCollapsed)
        {
            // 아래 방향으로 이동
            transform.Translate(Vector3.down * collapseSpeed * Time.deltaTime);
        }
    }

    // 붕괴 시퀀스 시작
    private IEnumerator StartCollapseSequence()
    {
        // 붕괴 전 대기 시간
        yield return new WaitForSeconds(warningDelay);

        // 붕괴 시작
        isCollapsing = true;

        // 일정 시간 후 오브젝트 비활성화
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }

    // 직접 붕괴 트리거 (에디터나 다른 스크립트에서 호출 가능)
    public void ForceCollapse()
    {
        if (!isCollapsing && !hasCollapsed && isCollapsible)
        {
            StartCoroutine(StartCollapseSequence());
        }
    }

    // 붕괴 큐브 초기화 (재사용 시)
    public void Reset()
    {
        StopAllCoroutines();
        isCollapsing = false;
        hasCollapsed = false;
    }

    // 디버그용: 씬에서 트리거 영역 시각화
    void OnDrawGizmos()
    {
        if (triggerType == TriggerType.PlayerProximity)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 주황색, 반투명
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}