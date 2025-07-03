using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간단한 글루 활성화 컴포넌트
/// OnCollisionEnter로 글루 활성화, 큐브 정지 시 자동 비활성화
/// </summary>
public class CubeGlueActivator : MonoBehaviour
{
    [Header("글루 설정")]
    [Tooltip("글루할 오브젝트 태그들")]
    public List<string> glueTags = new List<string> { "Player", "Monster" };

    // 컴포넌트 참조
    private CubeMover cubeMover;

    void Awake()
    {
        cubeMover = GetComponent<CubeMover>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 이동 중이 아니면 무시
        if (!cubeMover.IsCurrentlyMoving) return;

        // 태그 확인
        if (!IsValidTarget(collision.transform)) return;

        // 큐브 위에 있는지 확인 (옆면 충돌 무시)
        if (collision.transform.position.y < transform.position.y + 0.3f) return;

        // 글루 활성화
        cubeMover.EnableGlue();
    }

    void OnCollisionExit(Collision collision)
    {
        // 태그 확인
        if (!IsValidTarget(collision.transform)) return;

        // 잠시 후 글루 비활성화 (다른 오브젝트가 없으면)
        StartCoroutine(DelayedDisableGlue());
    }

    private bool IsValidTarget(Transform target)
    {
        foreach (string tag in glueTags)
        {
            if (target.CompareTag(tag)) return true;
        }
        return false;
    }

    private IEnumerator DelayedDisableGlue()
    {
        yield return new WaitForSeconds(0.1f);

        // 아직 다른 오브젝트와 충돌 중인지 확인하는 로직이 필요하면 추가
        // 지금은 간단하게 비활성화
        if (cubeMover.IsCurrentlyMoving)
        {
            cubeMover.DisableGlue();
        }
    }

    // CubeMover에서 정지 시 호출됨
    void OnDisable()
    {
        if (cubeMover != null)
        {
            cubeMover.DisableGlue();
        }
    }
}