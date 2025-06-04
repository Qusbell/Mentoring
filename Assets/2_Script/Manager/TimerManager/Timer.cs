using UnityEngine;
using System;
using System.Collections;


public class Timer : MonoBehaviour
{
    // �븮�� ����
    public delegate void TimerCallback();

    // ���� Ÿ�̸� �ڷ�ƾ
    // p_duration : Ÿ�̸� �ð�
    // p_callback : Ÿ�̸� ���� �� �����ų �Լ� (���� : ���� void, �Ű����� ����)
    public static IEnumerator StartTimer(float p_duration = 0, TimerCallback p_callback = null)
    {
        yield return new WaitForSeconds(p_duration);
        p_callback?.Invoke(); // �ð� ���� �� �ݹ� ����
    }
}
