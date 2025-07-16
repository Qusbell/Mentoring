using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CubeCollapser 전용 간단한 트리거
/// 이 에리어를 triggerArea로 설정한 모든 CubeCollapser를 자동으로 찾아서 처리
/// 비활성화된 큐브도 지원
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

    // CollapseTrigger.cs 수정 - TriggerType 체크 추가

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // 한 번만 트리거 확인
            if (oneTimeUse && hasTriggered) return;

            hasTriggered = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 감지! 연결된 CubeCollapser들 찾는 중...");

            // 이 에리어를 triggerArea로 설정한 모든 CubeCollapser 찾기 (비활성화된 것도 포함)
            CubeCollapser[] allCollapsers = FindObjectsOfType<CubeCollapser>(true);

            int foundCount = 0;

            foreach (CubeCollapser collapser in allCollapsers)
            {
                // 이 에리어를 참조하고 AND AreaTrigger 모드인 CubeCollapser만 처리
                if (collapser != null &&
                    collapser.triggerArea == this.gameObject &&
                    collapser.triggerType == CubeCollapser.TriggerType.AreaTrigger)  // 이 조건 추가!
                {
                    foundCount++;

                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 연결된 AreaTrigger 큐브 발견: [{collapser.gameObject.name}] - 워닝딜레이 {collapser.warningDelay}초 후 붕괴 시작");

                    // 각 큐브의 워닝딜레이만큼 기다린 후 붕괴 시작
                    Timer.Instance.StartTimer(this, collapser.warningDelay, () => {
                        if (collapser == null) return;

                        // 큐브가 비활성화되어 있다면 활성화
                        if (!collapser.gameObject.activeInHierarchy)
                        {
                            collapser.gameObject.SetActive(true);
                            if (showDebugLog)
                                Debug.Log($"[{gameObject.name}] 워닝딜레이 완료 - 큐브 [{collapser.gameObject.name}] 활성화 후 붕괴");
                        }
                        else
                        {
                            if (showDebugLog)
                                Debug.Log($"[{gameObject.name}] 워닝딜레이 완료 - 큐브 [{collapser.gameObject.name}] 붕괴 시작");
                        }

                        // 붕괴 트리거 (AreaTrigger 모드에서 상태 무관하게 작동)
                        collapser.TriggerCollapse();
                    });
                }
                // Time 모드나 다른 모드는 무시
                else if (collapser != null &&
                         collapser.triggerArea == this.gameObject &&
                         collapser.triggerType != CubeCollapser.TriggerType.AreaTrigger)
                {
                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 큐브 [{collapser.gameObject.name}]는 {collapser.triggerType} 모드이므로 CollapseTrigger에서 처리하지 않음");
                }
            }

            if (showDebugLog)
            {
                if (foundCount > 0)
                    Debug.Log($"[{gameObject.name}] 총 {foundCount}개의 AreaTrigger 큐브 발견!");
                else
                    Debug.LogWarning($"[{gameObject.name}] 이 에리어를 참조하는 AreaTrigger 모드 CubeCollapser를 찾을 수 없습니다.");
            }
        }
    }
    // 트리거 상태 리셋 (재사용을 위해)
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 트리거 상태 리셋");
    }
    
    // 디버그: 현재 이 에리어를 참조하는 CubeCollapser 목록 출력
    [ContextMenu("연결된 CubeCollapser 목록 확인")]
    public void ShowConnectedCollapsers()
    {
        CubeCollapser[] allCollapsers = FindObjectsOfType<CubeCollapser>(true);
        int count = 0;
        
        Debug.Log($"[{gameObject.name}] 연결된 CubeCollapser 목록:");
        
        foreach (CubeCollapser collapser in allCollapsers)
        {
            if (collapser != null && collapser.triggerArea == this.gameObject)
            {
                count++;
                string status = collapser.gameObject.activeInHierarchy ? "활성화" : "비활성화";
                Debug.Log($"  {count}. [{collapser.gameObject.name}] - {status}, 워닝딜레이: {collapser.warningDelay}초");
            }
        }
        
        if (count == 0)
            Debug.LogWarning($"[{gameObject.name}] 이 에리어를 참조하는 CubeCollapser가 없습니다.");
        else
            Debug.Log($"[{gameObject.name}] 총 {count}개의 CubeCollapser가 연결되어 있습니다.");
    }
}