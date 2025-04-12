using UnityEngine;

// 트리거 오브젝트에 붙여서 충돌을 감지하고,
// 스폰 매니저에게 전달하는 역할을 하는 스크립트
public class CollisionReporter : MonoBehaviour
{
    public spawn spawnerManager; // 연결된 스폰 매니저 (필수)

    // 일반 충돌 발생 시 호출됨
    private void OnCollisionEnter(Collision collision)
    {
        // 스포너 매니저가 존재하고 활성화된 경우에만 충돌 처리
        if (spawnerManager != null && spawnerManager.isActiveAndEnabled)
        {
            // 충돌한 대상 정보를 스폰 매니저에 전달
            spawnerManager.OnCollisionTrigger(gameObject, collision.gameObject);
        }
    }

    // 트리거 충돌이 발생했을 때 호출됨 (옵션)
    private void OnTriggerEnter(Collider other)
    {
        // 스포너 매니저가 존재하고 활성화된 경우에만 충돌 처리
        if (spawnerManager != null && spawnerManager.isActiveAndEnabled)
        {
            spawnerManager.OnCollisionTrigger(gameObject, other.gameObject);
        }
    }
}
