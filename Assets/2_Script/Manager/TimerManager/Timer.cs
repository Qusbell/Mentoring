using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//==================================================
// 활성화/비활성화를 요구하는 경우 사용 가능한 타이머
// 이후, TimerManager으로 확장 예정
//==================================================
public class Timer : MonoBehaviour
{
    // 활성화 후, 재활성/재사용까지의 대기 시간
    public float waitingTime = 1f;

    // 활성 가능 여부
    [HideInInspector] public bool isCanActivate { get; protected set; } = true;


    // 타이머 활성화 시도
    // return  true : 활성화 가능 및 활성화
    // return false : 활성화 불가 (쿨타임 중)
    public bool TryStartTimer()
    {
        // 사용 가능한 상태라면: 타이머 시작
        if (isCanActivate)
        { StartCoroutine(StartTimer()); return true; }
        // 사용 불가능한 상태(이미 동작 중)라면: 실행 X
        else
        { return false; }
    }


    // 타이머
    protected IEnumerator StartTimer()
    {
        isCanActivate = false; // 사용 불가 상태
        yield return new WaitForSeconds(waitingTime); // 시간 경과
        isCanActivate = true;  // 사용 가능 상태
    }
}