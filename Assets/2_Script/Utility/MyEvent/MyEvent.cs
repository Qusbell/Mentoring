using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyEvent
{
    private List<System.Action> callbackMulti = new List<System.Action>();
    private List<System.Action> callbackOnce = new List<System.Action>();

    /// <summary>
    /// 모든 콜백 실행
    /// </summary>
    public void CallBack()
    {
        callbackMulti.RemoveAll(a => a == null);
        callbackOnce.RemoveAll(a => a == null);

        // --- 다회용 이벤트 실행 ---
        foreach (var action in callbackMulti.ToArray())
        { action?.Invoke(); }

        // --- 일회용 이벤트 실행 & 제거 ---
        foreach (var action in callbackOnce)
        { action?.Invoke(); }
        callbackOnce.Clear();
    }

    /// <summary>
    /// 다회용 콜백 등록
    /// </summary>
    public void AddMulti(System.Action action, bool isUnique = false)
    {
        if (action == null || (isUnique &&callbackMulti.Contains(action)))
        { return; }

        callbackMulti.Add(action);
    }

    /// <summary>
    /// 일회용 콜백 등록
    /// </summary>
    public void AddOnce(System.Action action, bool isUnique = false)
    {
        if (action == null || (isUnique && callbackOnce.Contains(action)))
        { return; }

        callbackOnce.Add(action);
    }

    /// <summary>
    /// 전부 초기화
    /// </summary>
    public void ClearAll()
    {
        callbackMulti.Clear();
        callbackOnce.Clear();
    }

}
