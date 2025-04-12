using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ŀ���� ������ �Ŵ��� Ŭ���� - ������Ʈ ���� ������ �����ϰ�, ���ǿ� ���� ������Ʈ�� Ȱ��ȭ��
public class spawn : MonoBehaviour
{
    // ���� ���ǰ� ���õ� ������ �����ϴ� Ŭ����
    [System.Serializable]
    public class SpawnData
    {
        public GameObject targetObject;          // ������ ������Ʈ (��Ȱ��ȭ�� ���¿��� ����)
        public TriggerType triggerType;          // ���� Ʈ���� ���� (�ð� or �浹)
        public float delayTime;                  // �ð� Ʈ������ ���, ��ٸ� �ð�
        public GameObject triggerColliderObject; // �浹 Ʈ������ ���, �浹 ������ ������Ʈ (�÷��̾ ���⿡ ������ ����)

        [HideInInspector] public float timer = 0f;        // �ð� Ʈ���� ������ ���� Ÿ�̸�
        [HideInInspector] public bool hasSpawned = false; // �̹� �����Ǿ����� ����
    }

    // ���� ���� ���� ���ǵ��� �迭�� ����
    public SpawnData[] spawnSettings;

    // �� �����Ӹ��� ���� Ȯ�� (�ð� Ʈ���� ó����)
    void Update()
    {
        foreach (var data in spawnSettings)
        {
            // �̹� ������ ������Ʈ�� ��ŵ
            if (data.hasSpawned) continue;

            // Ʈ���� Ÿ�Կ� ���� �б�
            switch (data.triggerType)
            {
                case TriggerType.TimeTrigger:
                    data.timer += Time.deltaTime; // �ð� ����
                    if (data.timer >= data.delayTime)
                    {
                        SpawnObject(data); // �ð� ���� ���� �� ����
                    }
                    break;

                case TriggerType.CollisionTrigger:
                    // �浹�� �ܺο��� OnCollisionTrigger�� ó����
                    break;
            }
        }
    }

    // �浹 ���� �� �ܺ�(CollisionReporter ��)���� ȣ��Ǵ� �Լ�
    public void OnCollisionTrigger(GameObject triggerObject, GameObject other)
    {
        // �浹�� ����� "Player" �±׸� ������ �ִ��� Ȯ��
        if (!other.CompareTag("Player")) return;

        // Ʈ���� ������Ʈ�� ��ġ�ϴ� ��� ���� �����͸� �˻�
        foreach (var data in spawnSettings)
        {
            // �浹 ���� + Ʈ���� ������Ʈ�� ��ġ + ���� �������� ���� ���
            if (!data.hasSpawned &&
                data.triggerType == TriggerType.CollisionTrigger &&
                data.triggerColliderObject == triggerObject)
            {
                SpawnObject(data); // �ش� ������Ʈ ����
            }
        }
    }

    // ������Ʈ ���� (Ȱ��ȭ ó��)
    void SpawnObject(SpawnData data)
    {
        if (data.targetObject != null)
        {
            data.targetObject.SetActive(true);  // ������Ʈ Ȱ��ȭ
            data.hasSpawned = true;             // �ߺ� ������ ���� ���� ���� ����
        }
    }
}

// Ʈ���� ���� Ÿ�� ���� - �ð� ��� �Ǵ� �浹 ���
public enum TriggerType
{
    TimeTrigger,      // �ð� Ʈ����: ���� �ð� ��� �� ������Ʈ ����
    CollisionTrigger  // �浹 Ʈ����: Ư�� ������Ʈ(Player)�� ����� �� ����
}
