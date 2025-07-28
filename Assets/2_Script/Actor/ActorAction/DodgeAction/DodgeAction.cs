using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DodgeAction : ActorAction
{
    private Rigidbody rigid;
    private Collider myCollider;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        // 기존 마찰값 백업
        originalMaterial = myCollider.material;

        // 마찰 0인 Material 생성
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;

        // 비활성화 상태
        this.enabled = false;

        // 기존 레이어 저장
        originalLayer = this.gameObject.layer;
    }


    //    // 대시 회복 속도
    //    [SerializeField] protected float dodgeRecupTime = 3;
    //    
    //    // 대시 스택
    //    [SerializeField] protected int dodgeMaxStatck = 2;
    //    [SerializeField] protected int _dodgeStack = 2;
    //    
    //    protected int dodgeStack
    //    {
    //        get
    //        { return _dodgeStack; }
    //        set
    //        {
    //            if (dodgeMaxStatck <= value)
    //            {
    //                Timer.Instance.StopEndlessTimer(this, "_Recup"); // 재생 종료
    //                _dodgeStack = dodgeMaxStatck;
    //            }
    //            else { _dodgeStack = value; }
    //        }
    //    }


    // 대시 거리
    [SerializeField] protected float dodgePower = 8;

    // 미끄러지기 시간
    [SerializeField] protected float dodgeSlideTime = 0.4f;

    [SerializeField] protected float dodgeAngle = 30f;

    // 닷지 코스트
    [field: SerializeField] public int dodgeCost { get; set; } = 1;

    // 프리펩에서 지정해서, 실제로 회전시킬 오브젝트
    [SerializeField] private GameObject ratateObjectWhenDodge;


    // 땃쥐 중?
    public bool isDodge { get; protected set; }

    // 마찰계수
    private PhysicMaterial originalMaterial;   // 원래 Material 저장
    private PhysicMaterial zeroFrictionMaterial;


    //  public bool isCanDash
    //  {
    //      get
    //      { return 0 < dodgeStack; }
    //  }

    private int originalLayer = -1;


    public void Dodge()
    {
        // --- 닷지 ---
        rigid.velocity = this.transform.forward * dodgePower;

        // --- 닷지 시 적용 물리/기울기 등 ---
        myCollider.material = zeroFrictionMaterial;  // 마찰계수 제거
        rigid.useGravity = false;                    // 중력 미사용
        ratateObjectWhenDodge.transform.Rotate(dodgeAngle, 0, 0); // 앞으로 기울기
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");

        this.enabled = true;
        isDodge = true;

        // --- n초 후 물리 적용 ---
        Timer.Instance.StartTimer(this, "_Dodge", dodgeSlideTime, () => { rigid.velocity = Vector3.zero; });
    }



    // 닷지가 멈출 속도
    private float dodgeStopSpeed = 1f;

    // 현재 x/z 벡터값
    Vector2 horizontalVelocity;

    // dodge 정지 판정
    private void FixedUpdate()
    {
        // 현재 속도에서 x,z축만 분리
        horizontalVelocity = new Vector2(rigid.velocity.x, rigid.velocity.z);

        // 속도가 작으면 대시 중지
        if (horizontalVelocity.sqrMagnitude < dodgeStopSpeed)
        {
            // 원상복구
            myCollider.material = originalMaterial;
            rigid.useGravity = true;
            ratateObjectWhenDodge.transform.Rotate(-dodgeAngle, 0, 0);
            this.gameObject.layer = originalLayer;

            // 타이머 종료
            Timer.Instance.StopEndlessTimer(this, "_Dodge");

            // 추가로 대시 종료 시 필요한 처리 (이펙트, 애니메이션 등)
            this.enabled = false;
            isDodge = false;
        }
    }
}
