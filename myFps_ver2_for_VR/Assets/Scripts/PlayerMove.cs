using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Valve.VR;

/// <summary>
/// 플레이어 캐릭터 이동 
/// </summary>

public class PlayerMove : MonoBehaviour
{
    // 이동 속도 변수
    public float moveSpeed = 3.5f;

    // shift 누르고 있는 동안 플레이어 이동 속도 증가
    public float runSpeed = 7f;

    // 캐릭터 컨트롤러 변수
    CharacterController cc;

    CharacterController camRigCC;

    CharacterController capsulePlayerCC;

    //중력 변수
    public float gravity = -10.0f;

    //수직 속력 변수
    float yVelocity = 0;

    //점프력 변수
    public float jumpPower = 2f;

    // 점프 상태 변수
    public bool isJumping = false;

    // 플레이어 체력 변수
    public int hp = 20;

    // 플레이어 최대 체력 변수
    int maxHp = 20;

    // hp 슬라이더 변수
    public Slider hpSlider;

    // 플레이어 피격시 Hit 효과 오브젝트   // Image
    public GameObject hitEffect;

    // 애니메이터 변수
    Animator anim;

    // vive vr 카메라를 위함
     public GameObject cameraRig;

      // vive 컨트롤러 입력 소스 정의
    public SteamVR_Input_Sources leftHand = SteamVR_Input_Sources.LeftHand;
    public SteamVR_Input_Sources rightHand = SteamVR_Input_Sources.RightHand;
    public SteamVR_Input_Sources any = SteamVR_Input_Sources.Any;

    // 액션
    public SteamVR_Action_Boolean trigger = SteamVR_Actions.default_InteractUI;  // 트리거 버튼
    public SteamVR_Action_Boolean trackPad = SteamVR_Actions.default_Teleport;  // 트랙패드 클릭
    public SteamVR_Action_Boolean trackPadTouch = SteamVR_Actions.default_TrackpadTouch;  // 트랙패드 터치 여부
    public SteamVR_Action_Vector2 trackPadPosition = SteamVR_Actions.default_TrackpadPosition;  // 트랙패드 터치 좌표

    Vector2 leftTrackPadPos;  // 왼쪽 컨트롤러 트랙패드 좌표 값
    Vector2 rightTrackPadPos;  // 오른쪽 컨트롤러 트랙패드 좌표 값

    //플레이어 역할을 할 캡슐
    public GameObject capsulePlayer;


    // Start is called before the first frame update
    void Start()
    {
        // 캐릭터 콘트롤러 컴포넌트 받아오기
        cc = GetComponent<CharacterController>();

        camRigCC = cameraRig.GetComponent<CharacterController>();
       

        // 애니메이터 받아오기
        anim = GetComponentInChildren<Animator>();
    }

   

    // Update is called once per frame
    void Update()
    {

        // 현재 플레이어 hp(%)를 hp슬라이더의 value에 반영 / 현재 hp를 최대hp로 나눈 값으로 반영
        if (GameManager.gm.gState == GameManager.GameState.Run)
        {
            hpSlider.value = (float)hp / (float)maxHp;
        }

        // 게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다.
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        // w,a,s,d 로 이동
        // spacebar = 수직 점프

        // 사용자 입력 받기
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 이동 방향 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

         Vector3 dirVR = new Vector3(0,0,0);  // camera용

        // 플레이어 이동 블렌딩 트리를 호출하고 벡터의 크기 값을 넘겨준다
      //  anim.SetFloat("MoveMotion", dir.magnitude);

        
        // 메인 카메라를 기준으로 방향을 변환 / Camera 클래스의 main 변수는 메인 카메라 오브젝트를 가리킴
        dir = Camera.main.transform.TransformDirection(dir);

        // 점프 중이었고, 다시 바닥에 착지 시
        if(isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            // 점프 전 상태로 초기화
            isJumping = false;

            // 캐릭터 수직 속도를 0으로 / 높은 곳에서 아래로 뛰어내릴 때 마치 순간 이동처럼 떨어지는 문제 해결 / 바닥에 닿아 있을 때는 sssss 의 값을 '0'으로 초기화해줘야 누적 속도 없어짐
            yVelocity = 0;
        }

        //2.2 점프 기능
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            // 캐릭터 수직 속도에 점프력 적용
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 2.3 캐릭터 수직 속도에 중력 값을 적용
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 왼쪽 shift 누르고 있는 동안 플레이어 이동속도 증가
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 6.5f;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = 3.5f;
        }


        // 컨트롤러 제어
        // 왼쪽 트리거 한 번 눌렀을때
        if(trigger.GetStateDown(leftHand)){
            Debug.Log("Left Trigger Clicked");
            moveSpeed = 7f;
        }

         // 왼쪽 트리거 릴리즈 될때  /  빨리이동하려면 트리거 버튼 누른 상태 유지해야 한다
        if(trigger.GetStateUp(leftHand)){
            Debug.Log("Left Trigger released");
            moveSpeed = 3.5f;
        }


         if(trackPad.GetState(leftHand)){
            leftTrackPadPos = trackPadPosition.GetAxis(any);
            Debug.LogFormat("left touch position = {0}", leftTrackPadPos);  // 트랙패드 좌표  x축 왼 = -1 오 = 1  /  y축 위 = 1  아래 = -1

            // 왼쪽 이동
            if(leftTrackPadPos.x <= -0.6 && leftTrackPadPos.y <= 0.4 && leftTrackPadPos.y >= -0.4 ){
                dirVR = new Vector3(-1, 0, 0);

            }  
            else if(leftTrackPadPos.x >= 0.6 && leftTrackPadPos.y <= 0.4 && leftTrackPadPos.y >= -0.4){  // 오른쪽 이동
                dirVR = new Vector3(1, 0, 0);


            }   
            else if(leftTrackPadPos.x <= 0.4 && leftTrackPadPos.x >= -0.4 && leftTrackPadPos.y >= 0.6){  // 위쪽 이동
                dirVR = new Vector3(0, 0, 1);
                

            }
            else if(leftTrackPadPos.x <= 0.4 && leftTrackPadPos.x >= -0.4 && leftTrackPadPos.y <= -0.6){ // 아래쪽 이동
                dirVR = new Vector3(0, 0, -1);
                
            }   

            dirVR = dirVR.normalized;
            //   anim.SetFloat("MoveMotion", dirVR.magnitude);

             
        // 메인 카메라를 기준으로 방향을 변환 / Camera 클래스의 main 변수는 메인 카메라 오브젝트를 가리킴
        dirVR = Camera.main.transform.TransformDirection(dirVR);
        }



        // 이동 속도에 맞춰 이동
        // transform.position += dir * moveSpeed * Time.deltaTime;
        // cc.Move(dir * moveSpeed * Time.deltaTime);


         dirVR.y = yVelocity;
        //cc.Move(dirVR * moveSpeed * Time.deltaTime); // VR ver
        camRigCC.Move(dirVR * moveSpeed * Time.deltaTime); // VR ver

       
    }

    // 플레이어가 공격 당할 때  /  적군의 공격력 변수를 전달 받음
    public void DamageAction(int damage)
    {
        //적군의 공격력 만큼 플레이어 체력 감소
        hp -= damage;

        // 플레이어 체력이 0보다 크면 피격 이펙트 실행
        if(hp > 0)
        {
            // 피격 이펙트 코루틴 실행
            StartCoroutine(PlayHitEffect());
        }
    }

    // 피격효과 코루틴 함수
    IEnumerator PlayHitEffect()
    {
        // 피격 UI 활성화
        hitEffect.SetActive(true);

        // 0.2초 대기
        yield return new WaitForSeconds(0.2f);

        // 피격 UI 비활성화
        hitEffect.SetActive(false);
    }

}
