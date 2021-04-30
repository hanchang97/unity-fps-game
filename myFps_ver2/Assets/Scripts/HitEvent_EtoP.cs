using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///  적이 플레이어 공격할 때
/// </summary>

public class HitEvent_EtoP : MonoBehaviour
    
{
    // 적 스크립트 컴포넌트를 사용하기 위한 변수
    public EnemyFSM efsm;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 플레이어에게 데미지를 입히기 위한 이벤트 함수
    public void PlayerHit()
    {
        efsm.AttackAction();
    }
}
