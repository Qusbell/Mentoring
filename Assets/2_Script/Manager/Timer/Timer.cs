using UnityEngine;
using System;
using System.Collections;


public class Timer : MonoBehaviour
{
    // 범용 타이머 코루틴
    // p_duration : 타이머 시간
    // p_callback : 타이머 종료 시 실행시킬 함수 (조건 : 리턴 void, 매개변수 없음)
    public static IEnumerator StartTimer(float p_duration, Action p_callback)
    {
        yield return new WaitForSeconds(p_duration);
        p_callback?.Invoke(); // 시간 종료 시 콜백 실행
    }


    public static IEnumerator StartTimer<T>(float p_duration, Action<T> p_callback, T param)
    {
        yield return new WaitForSeconds(p_duration);
        p_callback?.Invoke(param); // 시간 종료 시 콜백 실행
    }


    // 무한반복 코루틴
    public static IEnumerator EndlessTimer(float p_duration, Action p_callback)
    {
        while (true)
        {
            yield return new WaitForSeconds(p_duration);
            p_callback?.Invoke();
        }
    }



    // 메쉬 삭제용 타이머
    public static IEnumerator LerpTimer(float duration, Action<float> onUpdate, Action onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            onUpdate?.Invoke(t);
            yield return null;
        }
        onUpdate?.Invoke(1f); // 마지막에 완전히 1로 보정
        onComplete?.Invoke();
    }
}