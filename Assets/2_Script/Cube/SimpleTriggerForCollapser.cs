using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CubeCollapser 전용 간단한 트리거 (기존 TriggerArea와 분리)
/// </summary>
public class SimpleTriggerForCollapser : MonoBehaviour
{
    [Tooltip("붕괴시킬 CubeCollapser들")]
    public List<CubeCollapser> targetCollapsers = new List<CubeCollapser>();

    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 감지");

            foreach (CubeCollapser collapser in targetCollapsers)
            {
                if (collapser != null)
                {
                    collapser.TriggerCollapse();

                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 큐브 [{collapser.gameObject.name}] 붕괴 트리거");
                }
            }
        }
    }
}