using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 범용 영역 추적기 - 지정된 태그의 오브젝트들이 영역에 진입/이탈할 때를 추적
/// </summary>
public class AreaTracker : MonoBehaviour
{
    [Header("추적 설정")]
    [Tooltip("추적할 오브젝트의 태그들")]
    public List<string> targetTags = new List<string> { "Player" };

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = false;

    [Header("이벤트")]
    [Tooltip("오브젝트가 영역에 진입했을 때")]
    public UnityEvent<GameObject> onObjectEnter;

    [Tooltip("오브젝트가 영역에서 이탈했을 때")]
    public UnityEvent<GameObject> onObjectExit;

    [Tooltip("첫 번째 오브젝트가 진입했을 때 (한 번만)")]
    public UnityEvent<GameObject> onFirstObjectEnter;

    [Tooltip("마지막 오브젝트가 이탈했을 때 (영역이 비었을 때)")]
    public UnityEvent onAreaEmpty;

    // 내부 변수
    private List<GameObject> objectsInArea = new List<GameObject>();
    private bool hasFirstEntered = false;
    private Collider areaCollider;

    void Awake()
    {
        areaCollider = GetComponent<Collider>();
        if (areaCollider == null)
        {
            areaCollider = gameObject.AddComponent<BoxCollider>();
        }
        areaCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 여러 태그 중 하나라도 일치하는지 확인
        bool isTargetTag = targetTags.Exists(tag => other.CompareTag(tag));
        if (!isTargetTag) return;

        // 이미 일회용으로 트리거되었으면 무시
        if (oneTimeUse && hasFirstEntered) return;

        // 리스트에 추가 (중복 방지)
        if (!objectsInArea.Contains(other.gameObject))
        {
            objectsInArea.Add(other.gameObject);

            // 이벤트 발생
            onObjectEnter?.Invoke(other.gameObject);

            // 첫 진입 이벤트
            if (!hasFirstEntered)
            {
                hasFirstEntered = true;
                onFirstObjectEnter?.Invoke(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 여러 태그 중 하나라도 일치하는지 확인
        bool isTargetTag = targetTags.Exists(tag => other.CompareTag(tag));
        if (!isTargetTag) return;

        // 리스트에서 제거
        if (objectsInArea.Contains(other.gameObject))
        {
            objectsInArea.Remove(other.gameObject);

            // 이벤트 발생
            onObjectExit?.Invoke(other.gameObject);

            // 영역이 비었을 때 이벤트
            if (objectsInArea.Count == 0)
            {
                onAreaEmpty?.Invoke();
            }
        }
    }

    // null 오브젝트 정리 (오브젝트가 파괴되었을 때 대비)
    private void LateUpdate()
    {
        objectsInArea.RemoveAll(obj => obj == null);
    }

    /// <summary>
    /// 상태 초기화
    /// </summary>
    public void ResetTracker()
    {
        objectsInArea.Clear();
        hasFirstEntered = false;
    }

    // 프로퍼티들
    public int Count => objectsInArea.Count;
    public bool IsEmpty => objectsInArea.Count == 0;
    public bool HasTarget => objectsInArea.Count > 0;
    public List<GameObject> ObjectsInArea => new List<GameObject>(objectsInArea); // 복사본 반환

    // 특정 태그의 오브젝트 개수 반환
    public int GetCountByTag(string tag)
    {
        return objectsInArea.FindAll(obj => obj.CompareTag(tag)).Count;
    }

    // 특정 태그의 오브젝트들만 반환
    public List<GameObject> GetObjectsByTag(string tag)
    {
        return objectsInArea.FindAll(obj => obj.CompareTag(tag));
    }
}