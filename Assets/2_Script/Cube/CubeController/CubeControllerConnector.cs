using UnityEngine;
using System.Collections.Generic;


// CubeController���� �����Ű�� Ŀ����
public class CubeControllerConnector : MonoBehaviour
{
    private List<CubeController> controllerSequence = new List<CubeController>();

    // �� ť�� ��Ʈ�ѷ� �� ���� ����
    void Awake()
    {
        // ��� �ڽ� ��Ʈ�ѷ� ���� (��Ȱ�� ����)
        GetComponentsInChildren<CubeController>(true, controllerSequence);

        // �����: ã�� ��Ʈ�ѷ� �� ���
        Debug.Log($"Found {controllerSequence.Count} controllers.");

        // ���� ���� (��: �̸� ���� ����)
        //  controllerSequence.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        // ��Ʈ�ѷ� ����
        for (int i = 0; i < controllerSequence.Count - 1; i++)
        {
            CubeController current = controllerSequence[i];
            CubeController next = controllerSequence[i + 1];

            // ���� ��Ʈ�ѷ� ���� ����
            current.nextController = next;
            // �̺�Ʈ ����
            current.nextCubeControllerActivate.AddListener(next.StartController);
        }

        if (controllerSequence[0] != null)
        { controllerSequence[0].StartController(); }
        else
        { Debug.Log("�ڽ� ������Ʈ�� CubeController ������Ʈ �������� ����."); }
    }
}
