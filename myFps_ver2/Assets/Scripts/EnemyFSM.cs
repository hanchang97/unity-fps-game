using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
///  적군 이동
/// </summary>

public class EnemyFSM : MonoBehaviour
{
    // 적 상태 상수
    enum EnemyState
    {
        Idle,
        Move,
        Attack,
        Return,
        Damaged,
        Die
    }

    //적군 상태 변수
    EnemyState m_State;

    //적군 입장에서 플레이어 발견 범위
    float findDistance = 6f;

    // 플레이어의 트랜스폼
    Transform player;


    // 적군의 공격 가능 범위 / 공격 시작
    float attackDistance = 2.5f;

    // 적군 이동 속도
    public float moveSpeed = 5f;

    // 적군 평소 걷는 속도
    public float walkSpeed;

    // 적군 추격 시 뛰는 속도
    public float runningSpeed;

    // 적군  최대 체력
    public int maxHp = 15;

    // 적군 체력
    public int hp = 15;

    // 적군 hp Slider 변수
    public Slider hpSlider;

    // 적군 컨트롤러 컴포넌트
    CharacterController cc;

    // 일정한 시간 간격으로 공격
    // 누적 시간 
    float currentTime = 0;

    // 공격 딜레이 시간
    float attackDelay = 1f;

    // 적군 공격력
    public int attackPower = 2;


    // 적군의 초기 위치
    Vector3 originPos;

    // 초기 위치에서의 회전값
    Quaternion originRot;


    // 플레이어와 일정 수준 거리가 멀어지면 되돌아간다
    float moveDistance = 12f;


    // 애니메이터 변수
    Animator anim;

    // 내비게이션 에이전트 변수
    NavMeshAgent smith;


    //Item 프리팹
    // rifle 
    public GameObject RiflePrefab;
    public GameObject SniperPrefab;
    public GameObject GrenadePrefab;
    public GameObject HpPrefab;


    // 적군 평소에 걸어다닐 때 목적지 위치 배열
    public Transform[] walkingPoints;

    // 한 목적지로 걷는 중
    bool isWalking = true;

    // 목적지 배열 index
    int destinationIdx;

    //좀비 idle 상태 사운드 리스트
    public List<AudioClip> idleAudioClipList = new List<AudioClip>();

    // 좀비 idle 상태 사운드
    public AudioClip zombieIdle1;
    public AudioClip zombieIdle2;
    public AudioClip zombieIdle3;

    // 좀비 피격 사운드
    public AudioClip zombieDamagedSound;

    // 좀비 audio source
    AudioSource zombieAS;

    //
    int idleIndex;

    bool isPlayingSound = false;




    // Start is called before the first frame update
    void Start()
    {
        // 적군 생성 시 상태는 대기상태
        m_State = EnemyState.Idle;

        // 게임 시작 시 플레이어 오브젝트의 트랜스폼 컴포넌트를 받아온다
        player = GameObject.Find("Player").transform;

        // 적군 오브젝트의 캐릭터 컨트롤러 컴포넌트 받아오기
        cc = GetComponent<CharacterController>();

        // 적군이 자신의 초기 위치 저장
        originPos = transform.position;
        originRot = transform.rotation;

        // 자식 오브젝트로부터 애니메이터 변수 받아오기 / Enemy 오브젝트 자식에 애니메이터 있음!
        anim = transform.GetComponentInChildren<Animator>();

        // 내비게이션 에이전트 컴포넌트 받아오기
        smith = GetComponent<NavMeshAgent>();

        idleAudioClipList.Add(zombieIdle1);
        idleAudioClipList.Add(zombieIdle2);
        idleAudioClipList.Add(zombieIdle3);
        

        zombieAS = GetComponent<AudioSource>();

        idleIndex = Random.Range(0, 3);
        zombieAS.clip = idleAudioClipList[idleIndex];
    }

    // Update is called once per frame
    void Update()
    {
        //현재 상태를 체크하여 해당 상태별로 정해진 움직임 수행
        switch (m_State)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:    // 피격 상태, 사망 상태 함수는 매 프레임마다 반복 실행이 아니라 1회만 실행되어야 하므로
                //Damaged();
                break;
            case EnemyState.Die:
                //Die();
                break;
        }

        // 적군 현재 hp(%)를 hp 슬라이더의 value에 반영
        hpSlider.value = (float)hp / (float)maxHp;
    }

    // 적군 대기 상태
    void Idle()
    {
        // Idle 상태에서 좀비 사운드
        if (!isPlayingSound)
        {
            isPlayingSound = true;
            //zombieAS.Play();
        }

        // 플레이어와의 거리가 동작 시작 범위 이내라면 Move 상태로 전환
        if (Vector3.Distance(transform.position, player.position) < findDistance)   // 평소에 리스폰 주위 지역 랜덤으로 돌아다니는 기능 추가해보기
        {
            m_State = EnemyState.Move;
            print("적군 상태 전환: Idle -> Move");

            // 이동 애니메션으로 전환하기
            anim.SetTrigger("IdleToMove");
        }
        else  // 돌아다니게 해보기
        {
            if (isWalking)
            {
                isWalking = false;
                // 해당 오브젝트 리스폰 지역외에 다른 리스폰 지역으로 이동하기
                int idx = Random.Range(0, walkingPoints.Length);
                destinationIdx = idx;

                smith.isStopped = true;
                smith.ResetPath();

                // 내비게이션 목적지를 랜덤위치로 설정
                smith.destination = walkingPoints[idx].position;
                // smith.speed = 0.75f;
                smith.speed = walkSpeed;
            }

            if (Vector3.Distance(transform.position, walkingPoints[destinationIdx].position) <= 1f)
            {
                isWalking = true;
            }
        }
    }

    void Move()   //적군 입장에서 공격 범위 안에 들어왔을 때와 공격 범위 안에 플레이어가 들어오지 않았을 때 두 가지 경우
    {

        /*if(Vector3.Distance(transform.position, player.position) > moveDistance) // 플레이어와의 거리가 일정 거리 이상 멀어지면 적은 원래 위치로 복귀
        {
            // 현재 상태를 복귀(Return)로 전환
            m_State = EnemyState.Return;
            print("적군 상태 전환: Move -> Return");

        }*/
       if(Vector3.Distance(transform.position, player.position) > attackDistance)  // 플레이어와의 거리가 공격 범위 밖이라면 플레이어를 향해 이동
        {
            // 이동 방향 / 플레이어를 바라보는 방향
            //Vector3 dir = (player.position - transform.position).normalized;

            // 캐릭터 컨트롤러를 이용해 이동
            //cc.Move(dir * moveSpeed * Time.deltaTime);

            // 플레이어를 향해 방향 전환
            //transform.forward = dir;



            // 기존의 캐릭터 컨트롤러로 이동하는 방식을 에이전트 이동 방식으로 변경

            // 내비게이션 에이전트의 이동을 멈추고 경로를 초기화한다 /  이동 상태에서 공격 상태로 전환할 때를 위해

            // 밑의 두 줄 활성화 하면 계속 path 업데이트 되면서 제자리에서 뛰는 오류 발생 가능성
            // smith.isStopped = true;
            // smith.ResetPath();

            // 내비게이션으로 접근하는 최소 거리를 공격 가능 거리로 설정한다
            smith.stoppingDistance = attackDistance;

            // 내비게이션 목적지를 플레이어의 위치로 설정
            smith.destination = player.position;
            //smith.speed = 4f;
            smith.speed = runningSpeed;

            
        }
        else    // 공격 가능 범위라면 공격 상태로 전환
        {
            m_State = EnemyState.Attack;
            print("적군 상태 전환: Move -> Attack");


            //누적 시간을 공격 딜레이 시간만큼 미리 진행 -> 첫 공격이 바로 실행되도록 하기 위함
            currentTime = attackDelay;

            // 공격 대기 애니메이션 실행
            anim.SetTrigger("MoveToAttackDelay");

            // 이동 멈추고 타겟 초기화
            smith.isStopped = true;
            smith.ResetPath();
        }
    }

    void Attack()  // 공격의 경우 플레이어가 도망가서 적군의 공격 범위 밖으로 벗어나는 경우와 플레이어가 공격 범위 안에 있을 때 두 가지 경우
    {
       // 플레이어가 범위 내에 있어 적군이 공격 가능할 때
       if(Vector3.Distance(transform.position, player.position) < attackDistance)
        {
            // 일정 시간마다 플레이어를 공격
            currentTime += Time.deltaTime;

            if(currentTime > attackDelay)
            {
                
                // player.GetComponent<PlayerMove>().DamageAction(attackPower); // PlayerMove 컴포넌트를 가져와 그 안에 구현한 피격 함수를 실행

                print("적군이 플레이어 공격");
                currentTime = 0;

                // 공격 애니메이션 플레이
                anim.SetTrigger("StartAttack");
            }
        }
        else  // 플레이어가 공격 가능 범위 밖에 있어 적군이 쫓아간다 / 재추격
        {
            m_State = EnemyState.Move;
            print("적군 상태 전환: Attack -> Move");
            currentTime = 0;

            // 재추격 이동 애니메이션 실행
            anim.SetTrigger("AttackToMove");
        }
    }

    // 플레이어의 스크립트의 데미지 처리 함수를 실행하기
    public void AttackAction()
    {
        player.GetComponent<PlayerMove>().DamageAction(attackPower);
    }

    void Return()
    {
        // 만약 초기 위치에서 거리가 0.1f 이상이면 기존 초기 위치로 이동
        if(Vector3.Distance(transform.position, originPos) > 0.1f)
        {
            // 내비게이션으로 변경
            //Vector3 dir = (originPos - transform.position).normalized;  // dir = 원래 위치로 향하는 방향
            //cc.Move(dir * moveSpeed * Time.deltaTime);

            // 보는 방향을 복귀 지점으로
            //transform.forward = dir;

            // 내비게이션의 목적지를 초기 저장된 위치로 설정
            smith.destination = originPos;

            //내비게이션으로 접근하는 최소 거리를 '0'으로 설정
            smith.stoppingDistance = 0;

        }
        else // 그렇지 않다면 현재 위치를 기존 초기 위치로 조정하고 대기 상태로 전환
        {
            // 내비게이션 에이전트의 이동을 멈추고 경로를 초기화
            smith.isStopped = true;
            smith.ResetPath();
            //

            transform.position = originPos;
            transform.rotation = originRot;

            // 체력 회복
            //hp = maxHp;  // 일단 적군 체력 회복 제외
            m_State = EnemyState.Idle;
            print("적군 상태 전환: Return -> Idle");

            // 대기 상태 애니메이션으로 전환하는 트랜지션 호출
            anim.SetTrigger("MoveToIdle");
        }
    }

    void Damaged() // 적군이 공격 받았을 때
    {
        //피격 상태를 처리하기 위한 코루틴 실행
        StartCoroutine(DamageProcess());
    }

    void Die()  // 적군 사망 상태에서는 이미 실행 중인 피격 코루틴이 있으면 모두 종료해서 갑자기 이동 상태로 전환되지 않게 한다
    {
        // 진행 중인 피격 코루틴을 중지
        StopAllCoroutines();

        //사망 상태를 처리하기 위한 코루틴 실행
        StartCoroutine(DieProces());
    }

    // 데미지 처리용 코루틴 메소드
    IEnumerator DamageProcess()
    {
        //피격 모션 시간만큼 기다린다
        yield return new WaitForSeconds(1.0f);

        //현재 상태를 이동 상태로 전환
        m_State = EnemyState.Move;
        print("상태 전환: Damaged -> Move");
    }

    // 데미지 실행 메소드
    public void HitEnemy(int hitPower)
    {
        // 이미 피격 상태이거나 사망 상태 또는 복귀 상태라면 아무런 처리도 하지 않고 함수 종료
        if(m_State == EnemyState.Die)
        {
            return;
        }


        // 플레이어 공격력 만큼 적군 체력 감소
        hp -= hitPower;

        // 피격 사운드
        zombieAS.clip = zombieDamagedSound;
        zombieAS.Play();

        // 내비게이션 에이전트의 이동을 멈추고 경로를 초기화
        smith.isStopped = true;
        smith.ResetPath();

        // 적군 체력이 0보다 크면 피격 상태로 전환
        if(hp > 0)
        {
            m_State = EnemyState.Damaged;
            print("적군 상태 전환: Any state -> Damaged");

            // 적군 피격 애니메이션 실행
            anim.SetTrigger("Damaged");

            Damaged();
        }
        else //그렇지 않다면 적군 사망
        {
            m_State = EnemyState.Die;
            print("적군 상태 전환: Any state -> Die");

            // 적군 사망 애니메이션 실행
            anim.SetTrigger("Die");

            Die();
        }
    }

    //  사망 상태 코루틴
    IEnumerator DieProces()
    {
       // GameManager.currentEnemyNum--;  // 적군 개체 수 감소

        // 적군 사망 위치에서 랜덤으로 아이템 생성
        int idx = Random.Range(0, 4);

        switch (idx) {
            case 0:
            Instantiate(RiflePrefab, transform.position, transform.rotation);
             break;
            case 1:
                Instantiate(GrenadePrefab, transform.position, transform.rotation);
                break;
            case 2:
                Instantiate(SniperPrefab, transform.position, transform.rotation);
                break;
            case 3:
                Instantiate(HpPrefab, transform.position, transform.rotation);
                break;
        }

        // 캐릭터 콘트롤러 컴포넌트 비활성화
        cc.enabled = false;

        // 2초 동안 기다린 후 자기 자신 제거
        yield return new WaitForSeconds(2f);
        print("사망한 적군 소멸");
        Destroy(gameObject);
    }
}
