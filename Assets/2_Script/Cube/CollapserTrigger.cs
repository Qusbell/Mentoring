using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐시 없는 간단한 CollapseTrigger
/// 워닝딜레이만 담당하고 큐브 활성화는 하지 않음
/// </summary>
public class CollapseTrigger : MonoBehaviour
{
    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (oneTimeUse && hasTriggered) return;
            hasTriggered = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 감지!");

            // 캐시 없이 바로 검색 및 처리
            CubeCollapser[] allCollapsers = FindObjectsOfType<CubeCollapser>(true);

            int processedCount = 0;
            foreach (CubeCollapser collapser in allCollapsers)
            {
                if (collapser != null &&
                    collapser.triggerArea == this.gameObject &&
                    collapser.triggerType == CubeCollapser.TriggerType.AreaTrigger)
                {
                    processedCount++;

                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 타이머 시작: {collapser.gameObject.name}, 딜레이: {collapser.warningDelay}초");

                    // 람다 캡처 문제 해결
                    CubeCollapser currentCollapser = collapser;
                    string uniqueKey = $"{currentCollapser.gameObject.GetInstanceID()}_{Time.time}_{processedCount}";

                    Timer.Instance.StartTimer(this, uniqueKey, currentCollapser.warningDelay, () => {
                        if (currentCollapser != null && currentCollapser.gameObject.activeInHierarchy)
                        {
                            currentCollapser.TriggerCollapse();

                            if (showDebugLog)
                                Debug.Log($"[{gameObject.name}] 붕괴 실행됨: {currentCollapser.gameObject.name}");
                        }
                        else if (showDebugLog)
                        {
                            Debug.Log($"[{gameObject.name}] 큐브 비활성화 상태라서 붕괴 안함: {currentCollapser?.gameObject.name}");
                        }
                    });
                }
            }

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] {processedCount}개 큐브 처리 완료!");
        }
    }

    // 트리거 상태 리셋
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}