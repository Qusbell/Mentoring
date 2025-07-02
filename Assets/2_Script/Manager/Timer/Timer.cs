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


}