using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events; // �̺�Ʈ �ý��� ����� ���� �߰�

/// <summary>
/// ť�� Ȱ��ȭ�� Ʈ���Ÿ� �����ϴ� ������Ʈ
/// </summary>
public class CubeController : MonoBehaviour
{

    // ���� Ʈ���� -> �������� �˻� (������ �۵��� ������)
    // �ð� Ʈ���� -> ������ �˻� (�� ������Ʈ����)


    // -------------------- �ʱ�ȭ --------------------

    // ���� �� ��� ť�� Ȯ��
    void Start()
    {
        // ���� �����̰� 0�̸� �ٷ� ���� ���·� ����
        if (startDelay <= 0f)
        {
            delayPassed = true;
            Debug.Log($"[{gameObject.name}] ������ ���� �ٷ� �����մϴ�.");
        }
        else
        { Debug.Log($"[{gameObject.name}] {startDelay}�� �Ŀ� ���۵˴ϴ�."); }

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
    // <- MoverAdder
    private void CheckAndAddMoverComponent(GameObject cube)
    {
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover == null)
        {
            Debug.LogWarning($"ť�� '{cube.name}'�� CubeMover ������Ʈ�� �����ϴ�. �ڵ����� �߰��˴ϴ�.");
            cube.AddComponent<CubeMover>();
        }
    }





    // -------------------- ������ --------------------

    [Header("��ü ���� ����")]
    [Tooltip("��ũ��Ʈ ���� �� ��� �ð� (��)")]
    public float startDelay = 0f;

    // ���� ������ Ÿ�̸�
    private float startDelayTimer = 0f;

    // ������ Ÿ�̸� ��� ����
    private bool delayPassed = false;

    // ���� ������ ���� (�ܺο��� ȣ�� ����)
    public void SetStartDelay(float delayInSeconds)
    {
        startDelay = Mathf.Max(0f, delayInSeconds); // ���� ����
        ResetAll(); // ������ ���������� �ʱ�ȭ
    }


    // -------------------- Ʈ���� --------------------

    // Ʈ���� ���� Ÿ�� ����
    public enum TriggerType
    {
        TimeTrigger,  // �ð� Ʈ����: ���� �ð� ��� �� ������Ʈ Ȱ��ȭ
        AreaTrigger,  // ���� Ʈ����: Ư�� ������ �÷��̾ ������ Ȱ��ȭ
        Manual        // ���� Ʈ����: �ڵ忡�� ���� ȣ���Ͽ� Ȱ��ȭ
    }

    [Header("�Ϸ� �̺�Ʈ")]
    [Tooltip("��� ť�갡 Ȱ��ȭ�Ǹ� �߻��ϴ� �̺�Ʈ")]
    public UnityEvent onAllCubesActivated;

    // ��� ť�� Ȱ��ȭ �˸�
    private bool hasTriggeredEvent = false;


    // ���� Ʈ���� ���� �� ȣ���
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        // �����̰� ������ �ʾ����� Ʈ���� ����
        if (!delayPassed) return;

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

    // ��� ť�갡 Ȱ��ȭ�Ǿ����� Ȯ��
    private void CheckAllCubesActivated()
    {
        // �̹� �̺�Ʈ�� Ʈ���������� ��ŵ
        if (hasTriggeredEvent) return;

        // ��� ť�갡 Ȱ��ȭ�Ǿ����� Ȯ��
        // <- activatedCubeCount�� activationSettings.count �� �񱳷� �ٲٱ�
        bool allActivated = true;
        foreach (var data in activationSettings)
        {
            if (!data.hasActivated)
            {
                allActivated = false;
                break;
            }
        }

        // ��� ť�갡 Ȱ��ȭ�Ǿ����� �̺�Ʈ �߻�
        if (allActivated && activationSettings.Length > 0)
        {
            hasTriggeredEvent = true;
            Debug.Log($"[{gameObject.name}] ��� ť�갡 Ȱ��ȭ�Ǿ����ϴ�. �̺�Ʈ�� �߻���ŵ�ϴ�.");

            // �̺�Ʈ �߻�
            onAllCubesActivated?.Invoke();
        }
    }
    

    // �� �����Ӹ��� �ð� Ʈ���� üũ
    void Update()
    {
        // �����̰� ���� ������ �ʾҴٸ� Ÿ�̸� ����
        if (!delayPassed)
        {
            startDelayTimer += Time.deltaTime;
            if (startDelayTimer >= startDelay)
            {
                delayPassed = true;
                Debug.Log($"[{gameObject.name}] ���� ������ {startDelay}�ʰ� �������ϴ�. ť�� Ȱ��ȭ�� �����մϴ�.");
            }
            else
            {
                return; // �����̰� ������ �ʾ����� ť�� Ȱ��ȭ ���� ���� �� ��
            }
        }


        // �����̰� �������� ť�� Ȱ��ȭ ���� ó��
        foreach (var data in activationSettings)
        {
            // �̹� Ȱ��ȭ�� ť��� ��ŵ
            if (data.hasActivated) { continue; }

            // �ð� Ʈ���� ó��
            if (data.triggerType == TriggerType.TimeTrigger)
            {
                data.timer += Time.deltaTime;
                if (data.timer >= data.delayTime)
                { ActivateCube(data); }
            }
        }

        // ��� ť�� Ȱ��ȭ üũ
        CheckAllCubesActivated();
    }




    // -------------------- Ȱ��ȭ --------------------

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

        // ����� �ð�
        [HideInInspector] public float timer = 0f;

        // Ȱ��ȭ ����
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("ť�� Ȱ��ȭ ����")]
    public CubeData[] activationSettings; // <- ����Ʈ�� �ٲٱ�

    // ���� Ȱ��ȭ�� ť���� ����
    private int activatedCubeCount = 0;


    // ť�� ���� Ȯ��
    private int CheckActivatedCubeCount()
    {
        int count = 0;

        foreach (CubeData data in activationSettings)
        {
            if(data.hasActivated)
            { count++; }
        }

        return count;
    }


    // ť�� Ȱ��ȭ
    private void ActivateCube(CubeData data)
    {
        // Ȱ��ȭ���� ���� ť����
        if (data.targetCube != null && !data.hasActivated)
        {
            // ť�� Ȱ��ȭ
            data.targetCube.SetActive(true);
            data.hasActivated = true;
            activatedCubeCount++;

            Debug.Log($"[{gameObject.name}] ť�� [{data.targetCube.name}]�� Ȱ��ȭ�Ǿ����ϴ�." +
                $" ({activatedCubeCount}/{activationSettings.Length})");
        }
    }


    // -------------------- �׽�Ʈ/����� --------------------

    [Header("����� �ɼ�")]
    [Tooltip("�� �����Ϳ��� ���� Ʈ���Ÿ� �ð�ȭ")]
    public bool showTriggerAreas = true;

    // ������ ��� �Ϸ� (�׽�Ʈ/����׿�)
    public void SkipDelay()
    {
        startDelayTimer = startDelay;
        delayPassed = true;
        Debug.Log($"[{gameObject.name}] ���� �����̸� �ǳʶپ����ϴ�.");
    }


    // ��� ť�� Ȱ��ȭ �̺�Ʈ ��� Ʈ���� (�׽�Ʈ/����׿�)
    public void TriggerAllCubesActivated()
    {
        if (!hasTriggeredEvent)
        {
            hasTriggeredEvent = true;
            onAllCubesActivated?.Invoke();
        }
    }

    // ��� ť�� ���� �ʱ�ȭ (�׽�Ʈ/����ۿ�)
    public void ResetAll()
    {
        // Ÿ�̸� �ʱ�ȭ
        startDelayTimer = 0f;
        delayPassed = startDelay <= 0f;
        hasTriggeredEvent = false;
        activatedCubeCount = 0;

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


    // Ư�� �ε����� ť�긦 �������� Ȱ��ȭ
    public void ActivateCubeByIndex(int index)
    {
        // �����̰� ������ �ʾ����� Ȱ��ȭ ����
        if (!delayPassed) return;

        // �ε��� ���� üũ
        if (index >= 0 && index < activationSettings.Length)
        { ActivateCube(activationSettings[index]); }
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