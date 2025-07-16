using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 캐싱 방식의 CollapseTrigger
/// 필수 기능만 유지, 디버그 로그 최소화
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

    // 캐싱 관련
    private List<CubeCollapser> cachedCollapsers = null;
    private bool cacheBuilt = false;

    void Start()
    {
        BuildCache();
    }

    // 캐시 구축
    private void BuildCache()
    {
        cachedCollapsers = new List<CubeCollapser>();

        CubeCollapser[] allCollapsers = FindObjectsOfType<CubeCollapser>(true);

        foreach (CubeCollapser collapser in allCollapsers)
        {
            if (collapser != null &&
                collapser.triggerArea == this.gameObject &&
                collapser.triggerType == CubeCollapser.TriggerType.AreaTrigger)
            {
                cachedCollapsers.Add(collapser);
            }
        }

        cacheBuilt = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 캐시 구축 완료: {cachedCollapsers.Count}개 큐브 등록");
    }

    // 캐시 갱신이 필요한지 체크
    private bool ShouldRefreshCache()
    {
        if (!cacheBuilt || cachedCollapsers == null) return true;

        // null 참조가 30% 넘으면 갱신
        int nullCount = 0;
        foreach (var collapser in cachedCollapsers)
        {
            if (collapser == null) nullCount++;
        }

        return cachedCollapsers.Count > 0 && (float)nullCount / cachedCollapsers.Count > 0.3f;
    }

    // null 참조 정리
    private void CleanupCache()
    {
        if (cachedCollapsers != null)
        {
            cachedCollapsers.RemoveAll(collapser => collapser == null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (oneTimeUse && hasTriggered) return;

            hasTriggered = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 감지!");

            // 캐시 상태 체크 및 갱신
            if (ShouldRefreshCache())
            {
                BuildCache();
            }
            else
            {
                CleanupCache();
            }

            // 캐시된 큐브들 처리
            int processedCount = 0;
            foreach (CubeCollapser collapser in cachedCollapsers)
            {
                if (collapser != null)
                {
                    processedCount++;

                    Timer.Instance.StartTimer(this, collapser.warningDelay, () => {
                        if (collapser != null && collapser.gameObject != null)
                        {
                            // 큐브 활성화 (필요시)
                            if (!collapser.gameObject.activeInHierarchy)
                            {
                                collapser.gameObject.SetActive(true);
                            }

                            collapser.TriggerCollapse();
                        }
                    });
                }
            }

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] {processedCount}개 큐브 처리 완료!");
        }
    }

    // 수동 캐시 갱신 (보스 스킬 등에서 큐브 생성 후 호출)
    public void RefreshCache()
    {
        BuildCache();
    }

    // 트리거 상태 리셋
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    // 디버그: 연결된 큐브 목록 확인
    [ContextMenu("연결된 큐브 목록 확인")]
    public void ShowConnectedCollapsers()
    {
        if (!cacheBuilt) BuildCache();

        Debug.Log($"[{gameObject.name}] 연결된 큐브 목록:");

        for (int i = 0; i < cachedCollapsers.Count; i++)
        {
            var collapser = cachedCollapsers[i];
            if (collapser != null)
            {
                string status = collapser.gameObject.activeInHierarchy ? "활성화" : "비활성화";
                Debug.Log($"  {i + 1}. [{collapser.gameObject.name}] - {status}, 딜레이: {collapser.warningDelay}초");
            }
            else
            {
                Debug.Log($"  {i + 1}. [NULL 참조]");
            }
        }
    }
}