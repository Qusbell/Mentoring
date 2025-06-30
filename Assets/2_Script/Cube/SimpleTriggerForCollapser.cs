using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CubeCollapser 전용 간단한 트리거 (기존 TriggerArea와 분리)
/// </summary>
public class SimpleTriggerForCollapser : MonoBehaviour
{
    [HideInInspector]
    public List<CubeCollapser> targetCollapsers = new List<CubeCollapser>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (CubeCollapser collapser in targetCollapsers)
            {
                if (collapser != null)
                {
                    collapser.TriggerCollapse();
                }
            }
        }
    }
}