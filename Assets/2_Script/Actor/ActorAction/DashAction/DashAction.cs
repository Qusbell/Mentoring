using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DashAction : ActorAction
{
    private Rigidbody rigid;
    private Collider myCollider;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        // 마찰 0인 임시 Material 생성
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
    }


    // 대시 거리
    [SerializeField] protected float dashPower = 8;

    // 대시 회복 속도
    [SerializeField] protected float dashRecupTime = 3;

    // 대시 스택
    [SerializeField] protected int dashStack = 2;

    // 미끄러지기 시간 (마찰계수 줄이기 시간)
    [SerializeField] protected float dashSlideTime = 0.2f;

    // 마찰계수
    private PhysicMaterial originalMaterial;   // 원래 Material 저장
    private PhysicMaterial zeroFrictionMaterial;

    public void Dash()
    {
        if (0 < dashStack)
        {
            // 대시
            rigid.AddForce(this.transform.forward * dashPower, ForceMode.Impulse);

            // 스택
            dashStack--;  // -1스택
            Timer.Instance.StartTimer(this, dashRecupTime, () => { dashStack++; }); // 일정 시간 후 다시 +1스택

            // 마찰계수 조정
            myCollider.material = zeroFrictionMaterial;
            Timer.Instance.StartTimer(this, "_Material", 0.2f, () => { myCollider.material = originalMaterial; });
        }
    }

}
