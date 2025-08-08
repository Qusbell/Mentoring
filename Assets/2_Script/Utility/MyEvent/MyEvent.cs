using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MyEvent
{
    private List<System.Action> multiCallbacks = new List<System.Action>();
    private List<System.Action> onceCallbacks = new List<System.Action>();

    /// <summary>
    /// 모든 콜백 실행
    /// </summary>
    public void Invoke()
    {
        // --- 다회용 이벤트 실행 ---
        foreach (var action in multiCallbacks.ToArray())
        { action?.Invoke(); }

        // --- 일회용 이벤트 실행 & 제거 ---
        foreach (var action in onceCallbacks.ToArray())
        { action?.Invoke(); }
        ClearOnceCallbacks();
    }

    // null action 제거
    public void Refresh()
    {
        multiCallbacks.RemoveAll((action) => { return action == null; });
        onceCallbacks.RemoveAll((action) => { return action == null; });
    }

    /// <summary>
    /// 다회용 콜백 등록
    /// </summary>
    public void AddMulti(System.Action action, bool isUnique = false)
    {
        if (action == null || (isUnique &&multiCallbacks.Contains(action))) { return; }
        multiCallbacks.Add(action);
    }

    /// <summary>
    /// 일회용 콜백 등록
    /// </summary>
    public void AddOnce(System.Action action, bool isUnique = false)
    {
        if (action == null || (isUnique && onceCallbacks.Contains(action))) { return; }
        onceCallbacks.Add(action);
    }

    /// <summary>
    /// 전부 초기화
    /// </summary>
    public void ClearAll()
    {
        ClearMultiCallbacks();
        ClearOnceCallbacks();
    }


    public void ClearOnceCallbacks()
    { onceCallbacks.Clear(); }

    public void ClearMultiCallbacks()
    { multiCallbacks.Clear(); }



    /// <summary>
    /// UI 호환을 위한 AddListener (AddMulti와 동일)
    /// </summary>
    public void AddListener(System.Action action)
    {
        AddMulti(action, true); // 중복 방지
    }


    /// <summary>
    /// UI 호환을 위한 RemoveListener
    /// </summary>
    public void RemoveListener(System.Action action)
    {
        if (action == null) return;

        multiCallbacks.Remove(action);
        onceCallbacks.Remove(action);
    }

}
