using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CallbackData
{
    private System.Action callback;
    private int count; // 남은 실행 횟수 (-1이면 무제한)

    public CallbackData(System.Action callback, int count)
    {
        this.callback = callback;
        this.count = count;
    }

    /// <summary>
    /// 콜백 실행 후 남은 횟수를 감소시키고
    /// 더 이상 실행 불가능하면 true 반환
    /// == true면 삭제
    /// </summary>
    public bool Invoke()
    {
        // --- 1. 콜백이 없으면 실행 불가능 → 삭제 대상 ---
        if (callback == null) { return true; }

        // --- 2. 콜백 실행 ---
        callback?.Invoke();

        // --- 3. 무제한 실행(-1)이면 카운트 감소 없이 계속 유지 ---
        if (count == -1) { return false; }

        // --- 4. 카운트 감소 -> 0이면 삭제 대상 ---
        return --count == 0;
    }

    // 이미 등록된 콜백인지 확인
    public bool IsOverlap(System.Action callback)
    { return this.callback == callback; }
}


public class MyCallBacks
{
    private List<CallbackData> callbacks = new List<CallbackData>();

    /// <summary>
    /// 모든 콜백 실행
    /// </summary>
    public void Invoke()
    {
        for (int i = callbacks.Count - 1; i >= 0; i--)
        {
            if (callbacks[i].Invoke()) // 실행
            { callbacks.RemoveAt(i); } // 더 이상 실행 불가능 시 : 삭제
        }
    }

    /// <summary>
    /// 콜백 등록
    /// callbackCount == -1이면 무제한
    /// </summary>
    public void Add(System.Action action, int callbackCount = -1, bool isUnique = false)
    {
        // --- action이 null이면 : 등록 X ---
        if (action == null) { return; }

        // --- 중복 체크 ---
        if (isUnique)
        {
            foreach (var callback in callbacks)
            {
                // 중복되었다면 : 등록 X
                if (callback.IsOverlap(action))
                { return; }
            }
        }

        // --- 등록 ---
        callbacks.Add(new CallbackData(action, callbackCount));
    }

    /// <summary>
    /// 특정 콜백 제거
    /// </summary>
    public void Remove(System.Action action)
    {
        if (action == null) return;
        callbacks.RemoveAll(cb => cb.IsOverlap(action));
    }

    public void Clear()
    { callbacks.Clear(); }
}

