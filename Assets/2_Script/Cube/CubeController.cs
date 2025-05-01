using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ť�� Ȱ��ȭ�� Ʈ���Ÿ� �����ϴ� ������Ʈ
/// </summary>
public class CubeController : MonoBehaviour
{
    // Ʈ���� ���� Ÿ�� ����
    public enum TriggerType
    {
        TimeTrigger,  // �ð� Ʈ����: ���� �ð� ��� �� ������Ʈ Ȱ��ȭ
        AreaTrigger,  // ���� Ʈ����: Ư�� ������ �÷��̾ ������ Ȱ��ȭ
        Manual        // ���� Ʈ����: �ڵ忡�� ���� ȣ���Ͽ� Ȱ��ȭ
    }

    // ť�� Ȱ��ȭ ������ �����ϴ� Ŭ����
    [System.Serializable]
    public class CubeData
    {
        [Header("������Ʈ ����")]
        [Tooltip("Ȱ��ȭ�� ť��")]
        public GameObject targetCube;

        [Header("Ʈ���� ����")]
        [Tooltip("Ȱ��ȭ Ʈ���� ����")]
        public TriggerType triggerType = TriggerType.TimeTrigger;

        [Tooltip("�ð� Ʈ������ ���, ��ٸ� �ð�")]
        public float delayTime = 0f;

        [Tooltip("���� Ʈ������ ���, �浹 ������ ���� ������Ʈ")]
        public GameObject triggerArea;

        [Tooltip("���� Ʈ������ ��� �±� (�⺻: Player)")]
        public string targetTag = "Player";

        [HideInInspector] public float timer = 0f;
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("ť�� Ȱ��ȭ ����")]
    public CubeData[] activationSettings;

    [Header("����� �ɼ�")]
    [Tooltip("�� �����Ϳ��� ���� Ʈ���Ÿ� �ð�ȭ")]
    public bool showTriggerAreas = true;

    // ���� �� ��� ť�� Ȯ��
    void Start()
    {
        foreach (var data in activationSettings)
        {
            if (data.targetCube != null)
            {
                // CubeMover ������Ʈ�� ������ �߰�
                CheckAndAddMoverComponent(data.targetCube);
            }
        }
    }

    // ť�꿡 CubeMover ������Ʈ�� �ִ��� Ȯ���ϰ� ������ �߰�
    private void CheckAndAddMoverComponent(GameObject cube)
    {
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover == null)
        {
            Debug.LogWarning($"ť�� '{cube.name}'�� CubeMover ������Ʈ�� �����ϴ�. �ڵ����� �߰��˴ϴ�.");
            cube.AddComponent<CubeMover>();
        }
    }

    // �� �����Ӹ��� ���� Ȯ�� (�ð� Ʈ���� ó����)
    void Update()
    {
        foreach (var data in activationSettings)
        {
            // �̹� Ȱ��ȭ�� ť��� ��ŵ
            if (data.hasActivated) continue;

            // �ð� Ʈ���� ó��
            if (data.triggerType == TriggerType.TimeTrigger)
            {
                data.timer += Time.deltaTime;
                if (data.timer >= data.delayTime)
                {
                    ActivateCube(data);
                }
            }
        }
    }

    // ���� Ʈ���� ���� �� ȣ���
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        // �� Ȱ��ȭ �����͸� Ȯ��
        foreach (var data in activationSettings)
        {
            // �̹� Ȱ��ȭ�� ť��� ��ŵ
            if (data.hasActivated) continue;

            // ���� Ʈ���� �����̰� ������ �±װ� ��ġ�ϴ��� Ȯ��
            if (data.triggerType == TriggerType.AreaTrigger &&
                data.triggerArea == triggerArea &&
                other.CompareTag(data.targetTag))
            {
                ActivateCube(data);
            }
        }
    }

    // ť�� Ȱ��ȭ
    void ActivateCube(CubeData data)
    {
        if (data.targetCube != null)
        {
            // ť�� Ȱ��ȭ
            data.targetCube.SetActive(true);
            data.hasActivated = true;
        }
    }

    // Ư�� �ε����� ť�긦 �������� Ȱ��ȭ
    public void ActivateCubeByIndex(int index)
    {
        if (index >= 0 && index < activationSettings.Length)
        {
            ActivateCube(activationSettings[index]);
        }
    }

    // ��� ť�� ���� �ʱ�ȭ (�׽�Ʈ/����ۿ�)
    public void ResetAll()
    {
        foreach (var data in activationSettings)
        {
            data.hasActivated = false;
            data.timer = 0f;

            if (data.targetCube != null && data.targetCube.activeSelf)
            {
                data.targetCube.SetActive(false);
            }
        }
    }

    // ����׿�: ������ ���� Ʈ���ſ� ť�긦 ������
    void OnDrawGizmos()
    {
        if (!showTriggerAreas || activationSettings == null) return;

        foreach (var data in activationSettings)
        {
            // Ʈ���� ���� ǥ��
            if (data.triggerType == TriggerType.AreaTrigger && data.triggerArea != null)
            {
                Collider triggerCollider = data.triggerArea.GetComponent<Collider>();
                if (triggerCollider != null)
                {
                    // ���� Ʈ���Ŵ� ������ �ڽ��� ǥ��
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
                }
            }
        }
    }
}