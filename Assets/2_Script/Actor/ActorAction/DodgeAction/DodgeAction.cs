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

    // 미끄러지기 시간 (마찰계수 줄이기 시간)
    [SerializeField] protected float dodgeSlideTime = 0.2f;

    // 콤보 넣기 시간
    [SerializeField] protected float dodgeComboTime = 0.4f;

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



    public void Dodge()
    {
        // ----- 닷지 -----
        rigid.velocity = Vector3.zero;
        rigid.AddForce(this.transform.forward * dodgePower, ForceMode.Impulse);

        // ----- 스택 -----
        //  dodgeStack--;  // -1스택
        //  Timer.Instance.StartEndlessTimer(this, "_Recup", dodgeRecupTime, () => { dodgeStack++; }); // 스택 재생 시작

        // ----- 닷지 시 적용 물리/기울기 등 -----
        myCollider.material = zeroFrictionMaterial;  // 마찰계수 제거
        rigid.useGravity = false;                    // 중력 미사용
        ratateObjectWhenDodge.transform.Rotate(dodgeAngle, 0, 0); // 앞으로 기울기

        int originalLayer = this.gameObject.layer;
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");

        Timer.Instance.StartTimer(this, "_DodgeTime", dodgeSlideTime,
            () => {
                // 원상복구
                myCollider.material = originalMaterial;
                rigid.useGravity = true;
                ratateObjectWhenDodge.transform.Rotate(-dodgeAngle, 0, 0);
                this.gameObject.layer = originalLayer;
                rigid.velocity = Vector3.zero; // 종료 시 힘 제거
            });

        // ----- 땃쥐 지속시간 (콤보 넣기 시간) -----
        isDodge = true;
        Timer.Instance.StartTimer(this, "_ComboTime", dodgeComboTime, () => { isDodge = false; });
    }
}
