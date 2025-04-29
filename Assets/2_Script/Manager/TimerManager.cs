using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//==================================================
// 활성화/비활성화를 요구하는 모든 종류의 타이머에서 사용 가능한 매니저
//==================================================
public class TimerManager : MonoBehaviour
{
    // 활성화 불가능한 시간
    [SerializeField]
    protected float CanNotActivateTime = 5f;

    // 활성 가능 여부
    [HideInInspector]
    public bool isCanActivate { get; protected set; } = true;
    

    // 타이머 시작 시도
    public void TryStartTimer()
    {
        // 사용 불가능한 상태(이미 동작 중)라면: 실행 X
        if (!isCanActivate) { return; }

        // 타이머 돌리기
        Timer();
    }

   
    // 타이머
    protected IEnumerator Timer()
    {
        isCanActivate = false; // 사용 불가 상태
        yield return new WaitForSeconds(CanNotActivateTime); // 시간 경과
        isCanActivate = true;  // 사용 가능 상태
    }
}