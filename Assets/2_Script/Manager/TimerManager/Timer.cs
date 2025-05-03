using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//==================================================
// Ȱ��ȭ/��Ȱ��ȭ�� �䱸�ϴ� ��� ��� ������ Ÿ�̸�
// ����, TimerManager���� Ȯ�� ����
//==================================================
public class Timer : MonoBehaviour
{
    // Ȱ��ȭ ��, ��Ȱ��/��������� ��� �ð�
    public float waitingTime = 1f;

    // Ȱ�� ���� ����
    [HideInInspector] public bool isCanActivate { get; protected set; } = true;


    // Ÿ�̸� Ȱ��ȭ �õ�
    // return  true : Ȱ��ȭ ���� �� Ȱ��ȭ
    // return false : Ȱ��ȭ �Ұ� (��Ÿ�� ��)
    public bool TryStartTimer()
    {
        // ��� ������ ���¶��: Ÿ�̸� ����
        if (isCanActivate)
        { StartCoroutine(StartTimer()); return true; }
        // ��� �Ұ����� ����(�̹� ���� ��)���: ���� X
        else
        { return false; }
    }


    // Ÿ�̸�
    protected IEnumerator StartTimer()
    {
        isCanActivate = false; // ��� �Ұ� ����
        yield return new WaitForSeconds(waitingTime); // �ð� ���
        isCanActivate = true;  // ��� ���� ����
    }
}