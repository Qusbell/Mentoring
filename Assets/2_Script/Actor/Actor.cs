using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



[RequireComponent (typeof (Rigidbody))]
abstract public class Actor : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    private Rigidbody _rigid;
    public Rigidbody rigid
    {
        get
        {
            if (_rigid == null)
            {
                _rigid = GetComponent<Rigidbody>();
                if (_rigid == null) { Debug.LogWarning($"{gameObject.name} 오브젝트에 Rigidbody 컴포넌트가 없습니다."); }
            }

            return _rigid;
        }
    }



    // 이동
    private MoveAction _moveAction;
    public MoveAction moveAction
    {
        get
        {
            if (_moveAction == null)
            {
                _moveAction = GetComponent<MoveAction>();
                if (_moveAction == null) { Debug.LogWarning($"{gameObject.name} 오브젝트에 MoveAction 컴포넌트가 없습니다."); }
            }

            return _moveAction;
        }
    }



    // 공격
    protected AttackAction attackAction
    {
        get
        {
            if (allAttackActions.TryGetValue(nowAttackKey, out var action))
            { return action; }
            else
            {
                Debug.LogWarning($"{gameObject.name} : 현재 AttackKey({nowAttackKey})에 해당하는 AttackAction이 없습니다.");
                return null;
            }
        }
    }

    // 공격 목록
    // 각 공격행동이 자신의 키를 보유
    private Dictionary<AttackName, AttackAction> allAttackActions = new Dictionary<AttackName, AttackAction>();

    // 모든 공격 key
    protected List<AttackName> allAttackKey = new List<AttackName>();

    // 현재의 공격 key
    protected AttackName nowAttackKey;


    // 피격
    private DamageReaction _damageReaction;
    public virtual DamageReaction damageReaction
    {
        get
        {
            if (_damageReaction == null)
            {
                _damageReaction = GetComponent<DamageReaction>();
                if (_damageReaction == null) { Debug.LogWarning($"{gameObject.name} 오브젝트에 DamageReaction 컴포넌트가 없습니다."); }  
            }
            return _damageReaction;
        }
    }


    // 애니메이션
    protected ActorAnimation _animator;

    public ActorAnimation animator
    {
        get
        {
            if( _animator == null)
            {
                _animator = GetComponent<ActorAnimation>();
                if (_animator == null) { Debug.LogWarning($"{gameObject.name} 오브젝트에 ActorAnimation 컴포넌트가 없습니다."); }
            }
            return _animator;
        }
    }


    // 바닥 콜라이더 (점프 판정) : 현재 Player 이외에 존재하지 않음
    private FootCollider _foot;
    public virtual FootCollider foot
    {
        get
        {
            if (_foot == null)
            {
                _foot = GetComponentInChildren<FootCollider>();
                if (_foot == null) { Debug.LogWarning($"{gameObject.name} 오브젝트에 FootCollider가 없습니다."); }
            }

            return _foot;
        }
    }

    // 점프 중 판정 확인
    public bool isRand
    { 
        get { return foot.isRand; }
    }

    // 마지막으로 착지해 있었던 위치
    public Vector3 lastestRandedPos
    {
        get { return foot.lastestRandedPos; }
    }


    // 생성 초기화
    protected virtual void Awake()
    {
        rigid.freezeRotation = true;

        // 공격 목록 만들기
        AttackAction[] tempAttackActions = GetComponents<AttackAction>();
        foreach (var item in tempAttackActions)
        {
            if(allAttackActions.ContainsKey(item.attackName))
            { Debug.LogError("중복된 공격 Name 할당 : " + this.gameObject.name + " : " + item.attackName); continue; }
            else
            { allAttackActions[item.attackName] = item; }
        }
    }
}