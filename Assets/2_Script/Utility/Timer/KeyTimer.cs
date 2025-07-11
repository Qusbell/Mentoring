using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class KeyTimer : SingletonT<KeyTimer>
{
    // 현재 실행 중인 타이머 저장
    // 단일 실행 보장용
    // key = (gameObject.GetInstanceID(), string)
    private Dictionary<(int, string), Coroutine> continuousTimers = new Dictionary<(int, string), Coroutine>();



    // ===== 타이머 (중복 방지) =====

    // 타이머 시작
    public void StartTimer((int, string) key, float duration, Action callback)
    {
        if (continuousTimers.ContainsKey(key)) { return; }

        Coroutine timer = StartCoroutine(TimerCoroutine(key, duration, callback));
        continuousTimers[key] = timer;
    }

    // 타이머 조기 종료
    public void StopTimer((int, string) key)
    {
        // 동일 키가 있는 경우에만 실행
        if (!continuousTimers.ContainsKey(key)) { return; }

        StopCoroutine(continuousTimers[key]);
        continuousTimers.Remove(key);
    }

    // 타이머
    protected IEnumerator TimerCoroutine((int, string) key, float duration, Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
        continuousTimers.Remove(key);
    }



    // ===== 무한반복 타이머 (중복방지) =====

    // 중복 실행 방지 EndlessTimer 시작
    public void StartEndlessTimer((int, string) key, float duration, Action callback)
    {
        var endlessKey = (key.Item1, key.Item2 + "_Endless");
        if (continuousTimers.ContainsKey(endlessKey)) { return; } // 이미 실행 중이면 무시

        Coroutine timer = StartCoroutine(EndlessTimerCoroutine(endlessKey, duration, callback));
        continuousTimers[endlessKey] = timer;
    }

    // EndlessTimer 중지
    public void StopEndlessTimer((int, string) key)
    {
        var endlessKey = (key.Item1, key.Item2 + "_Endless");
        if (!continuousTimers.ContainsKey(endlessKey)) { return; }

        StopCoroutine(continuousTimers[endlessKey]);
        continuousTimers.Remove(endlessKey);
    }

    // EndlessTimer 코루틴 (중복 실행 방지)
    private IEnumerator EndlessTimerCoroutine((int, string) key, float duration, Action callback)
    {
        while (true)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }
        // ※ 무한 반복이므로 Remove는 StopEndlessTimer에서만 호출
    }



    // ===== 타이머 (중복 가능) =====

    public void StartTimer(float duration, Action callback)
    {
        StartCoroutine(TimerCoroutine(duration, callback));
    }

    private IEnumerator TimerCoroutine(float duration, Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }

}