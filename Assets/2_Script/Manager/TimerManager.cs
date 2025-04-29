using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//==================================================
// Ȱ��ȭ/��Ȱ��ȭ�� �䱸�ϴ� ��� ������ Ÿ�̸ӿ��� ��� ������ �Ŵ���
//==================================================
public class TimerManager : MonoBehaviour
{
    // Ȱ��ȭ �Ұ����� �ð�
    [SerializeField]
    protected float CanNotActivateTime = 5f;

    // Ȱ�� ���� ����
    [HideInInspector]
    public bool isCanActivate { get; protected set; } = true;
    

    // Ÿ�̸� ���� �õ�
    public void TryStartTimer()
    {
        // ��� �Ұ����� ����(�̹� ���� ��)���: ���� X
        if (!isCanActivate) { return; }

        // Ÿ�̸� ������
        Timer();
    }

   
    // Ÿ�̸�
    protected IEnumerator Timer()
    {
        isCanActivate = false; // ��� �Ұ� ����
        yield return new WaitForSeconds(CanNotActivateTime); // �ð� ���
        isCanActivate = true;  // ��� ���� ����
    }
}