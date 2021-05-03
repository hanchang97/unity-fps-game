using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 수류탄 기능
/// </summary>

public class BombAction : MonoBehaviour
{
    //폭발 이펙트 프리팹 변수
    public GameObject bombEffect;

    public LayerMask m_TargetLayer;

    // 수류탄 데미지
    public int attackPower = 15;

    // 수류탄 폭발 반경
    public float explosionRadius = 5f;

    // 플레이어의 트랜스폼
    Transform player;

    // Start is called before the first frame update
    void Start()
    {
        // 게임 시작 시 플레이어 오브젝트의 트랜스폼 컴포넌트를 받아온다
        player = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //충돌 시 처리
    public void OnCollisionEnter(Collision collision)
    {
        // 폭발 효과 반경 내에서 레이어가 'Enemy'인 모든 게임 오브젝트들의 Collider 컴포넌트 배열에 저장
        Collider[] cols = Physics.OverlapSphere(transform.position, explosionRadius, 1<<11);

        // 저장된 Collider 배열에 있는 모든 Enemy에게 수류탄 데미지 적용
        for(int i = 0; i < cols.Length; i++)
        {
             Debug.Log(cols[i].gameObject.name);
             cols[i].GetComponent<EnemyFSM>().HitEnemy(attackPower);
        }

        // 이펙트 프리팹 생성
        GameObject eff = Instantiate(bombEffect);

        //  수류탄 충돌 시 사운드 이펙트
        player.GetComponent<PlayerFire>().GrenadeExplosion();


        // 이펙트 프리팹 위치는 수류탄 오브젝트 위치와 동일
        eff.transform.position = transform.position;


        //수류탄 자신을 제거
        Destroy(gameObject);
    }
}
