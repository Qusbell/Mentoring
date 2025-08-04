using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



[RequireComponent (typeof (Rigidbody))]
abstract public class Actor : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    public Rigidbody rigid { get; set; }

    // 이동
    protected MoveAction moveAction;

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
    DamageReaction _damageReaction;
    public DamageReaction damageReaction
    {
        get
        {
            if (_damageReaction == null)
            {
                _damageReaction = GetComponent<DamageReaction>();

                // 여전히 null인 경우
                if (_damageReaction == null)
                { Debug.LogWarning($"{gameObject.name} 오브젝트에 DamageReaction 컴포넌트가 없습니다."); }  
            }
            return _damageReaction;
        }
    }


    // 애니메이션
    protected ActorAnimation animator;

    // 바닥 콜라이더 (점프 판정) : 현재 Player 이외에 존재하지 않음
    protected FootCollider foot;

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
        // 물리연산 포함
        rigid = GetComponent<Rigidbody>();
        // 물리회전 제거
        rigid.freezeRotation = true;

        animator = GetComponent<ActorAnimation>();
        moveAction = GetComponent<MoveAction>();
        foot = GetComponentInChildren<FootCollider>();

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