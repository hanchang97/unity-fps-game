using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Valve.VR;

public class GameManager : MonoBehaviour
{
    // 싱글톤 변수
    public static GameManager gm;

    public static int PlayerColaCount = 0;


    // 게임 상태 UI 오브젝트 변수
    public GameObject gameLabel;

    // 타이머 시작
    public bool timeStart;

   

    private void Awake()
    {
        if(gm == null)
        {
            gm = this;
        }
    }

    // 게임 상태 상수
    public enum GameState
    {
        Ready,
        Run,
        Pause,
        GameOver
    }

    // 현재 게임 상태 변수
    public GameState gState;


    // 게임 상태 UI 텍스트 컴포넌트 변수
    Text gameText;

    // PlayerMove 클래스 변수
    PlayerMove player;

    // 옵션 화면 UI 오브젝트 변수
    public GameObject gameOption;


    // 적군 리스폰 위치 배열
    public Transform[] respawnPoints;

    // 적군 프리팹 할당할 변수
    public GameObject EnemyPrefab;

    // 보스급 적군 프리팹 변수
    public GameObject BossEnemyPrefab;

    // 적군 리스폰 주기
    public float respawnTime = 7;

    // 보스급 적군 리스폰 주기
    public float respawnTime_Boss = 14;

    // 적군 최대 수
    public int maxEnemyNum = 20;

    // 현재 적군 수
    public static int currentEnemyNum;


    // 시간 제한
    float LimitTime = 100;

    // 남은 시간 정보 텍스트
    public Text text_timer;
    public Text text_timer_vr;

    // 콜라 아이템 스폰 위치 배열
    public Transform[] ColaRespawnPoints;

    // 콜라 프리팹 변수
    public GameObject ColaPrefab;

    // 콜라 리스폰 주기
    public float colaRespawnTime = 8;

    // 최대 콜라 개수
    int maxCola = 8;

    // 현재 콜라 개수
    int currentCola = 0;

    // 게임 시작 시 콜라 리스폰 지역 shuffle
    bool shuffleCola = false;

    // 현재 획득 콜라 개수 Text
    public Text text_currentColaCount;
    //vr
    public Text text_current_coke_vr;

    // 현재 마우스 감도 Text
    public Text text_MouseSensitivity;

    // 현재 마우스 감도
    int mouseSensitivity = 5;


    // 플레이어 사망 시 카메라 이동위한 변수
    public Camera GameOverCam;

    // 메인 카메라
    public Camera MainCam;

    // 애니메이션 / 카메라
    Animation anim;

    // 플레이어 애니메이션 받기
    Animator anim_player;

    // 사망 애니메이션 한 번만 실행하기 위함
    bool isDead;


    // 시점 변환 위한 두개의 빈 오브젝트
    public GameObject TPS_pos;
    public GameObject FPS_pos;

    // 현재 fps인지 아닌지
    bool isFps;

    // 현재 시점
    public GameObject CamPosition;


    // 게임 최초 실행 시
    bool gameStart = false;


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


    // 게임오버 상태에서 나올 문구 + UI
    public GameObject gameOverUI; 
    // 게임 오버 or 게임 승리 나타내는 텍스트
    public Text gameOverWinText;



    // Start is called before the first frame update
    void Start()
    {
        // 스크린 해상도 고정
       // Screen.SetResolution(Screen.width, Screen.width * 16 / 9, true);

        currentCola = 0;
        PlayerColaCount = 0;
        currentEnemyNum = 0;

        Time.timeScale = 1f;

        // 게임 최초 상태는  준비 상태
        gState = GameState.Ready;

        //gameStart = true;

        // 게임 상태 UI 오브젝트에서 Text 컴포넌트를 가져온다
        gameText = gameLabel.GetComponent<Text>();

        // 상태 텍스트의 내용을 Ready로 한다
        gameText.text = "Ready...";

        // 상태 텍스트 색상 설정 / 주황
        gameText.color = new Color32(255, 185, 0, 255);

        // 게임 준비 -> 게임 중 상태로 전환
        StartCoroutine(ReadyToStart());

        // 플레이어 오브젝트를 찾은 후 플레이어의 PlayerMove 컴포넌트 받아오기
        player = GameObject.Find("Player").GetComponent<PlayerMove>();

        // 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        respawnPoints = GameObject.Find("EnemySpawnPoint").GetComponentsInChildren<Transform>();

        // 게임 오버 시 카메라
        // anim = GameOverCam.GetComponent<Animation>(); / 게임 오버 시 카메라 이동하는 것 우선 비활성화

        // anim_player = GameObject.Find("Player").GetComponentInChildren<Animator>();

        isDead = false;

        



        gState = GameState.Run;

        // 몬스터 생성 코루틴 함수 호출
      //  StartCoroutine(this.CreateEnemy());
      //  StartCoroutine(this.CreateBossEnemy());

        // 콜라 생성 코루틴 함수 호출
        shuffleCola = true;

        StartCoroutine(this.CreateCola());


    }

    // Update is called once per frame
    void Update()
    {
        // 획득 콜라 개수 update
        text_currentColaCount.text = PlayerColaCount.ToString();
        text_current_coke_vr.text = PlayerColaCount.ToString();


        // 플레이어 체력이 0 이하가 되면
        if(player.hp <= 0 || (LimitTime <= 0 && PlayerColaCount < 7))
        {
            // 플레이어의 애니메이션을 멈춘다
            // player.GetComponentInChildren<Animator>().SetFloat("MoveMotion", 0f);

            if (!isDead)
            {
                isDead = true;
                //anim_player.SetTrigger("PlayerDead");
            }

            //상태 텍스트 활성화
            gameOverUI.SetActive(true);

            // 게임오버로 내용변경
            gameOverWinText.text = "Game Over";

            // 상태 텍스트 색상
            gameOverWinText.color = new Color32(255, 0, 0, 255);

            // 체력바 0으로 만들기
            player.hpSlider.value = 0;

            // 상태 텍스트의 자식 오브젝트의 트팬스폼 컴포넌트를 가져오기
            Transform buttons = gameText.transform.GetChild(0);

            // 버튼 오브젝트 활성화 / 게임 오버 되었을 때의 restart 와 quit 버튼
            //buttons.gameObject.SetActive(true);
            


            // 타이머 중지
            timeStart = false;
            gState = GameState.GameOver;
            
            
        // 마우스 커서 활성화
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        
        //  씬 중지
        Time.timeScale = 0f;

            // 메인카메라 비활 / 플레이어 사망 애니메이션 보여주기 위한 카메라 활성화
            //GameOverCam.gameObject.SetActive(true);
            //MainCam.gameObject.SetActive(false);

            



            // 플레이어 사망 애니메이션 보여주기 위한 카메라 이동 애니메이션 실행
           // anim.Play();
            
            
            //
           // StartCoroutine(PlayerDeadDelay());


            /*
                        // 상태를 '게임 오버' 상태로 변경
                        gState = GameState.GameOver;

                        // 마우스 커서 활성화
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;

                        // 타이머 중지
                        timeStart = false;

                        //  씬 중지
                        Time.timeScale = 0f;
            */

        }

        // Tab키 누를 시 게임 일시 정지 및 개임 재개
        if (Input.GetKeyDown(KeyCode.Tab))
        {   
            if (gState == GameState.Run)
            {
                OpenOptionWindow();
            }
            else if(gState == GameState.Pause)
            {
                CloseOptionWindow();
            }
        }


        //오른쪽 트랙패드 정중앙 누를 시
        if(trackPad.GetStateDown(rightHand) && gState == GameState.Run){
                rightTrackPadPos = trackPadPosition.GetAxis(any); 

                if(rightTrackPadPos.x <= 0.4 && rightTrackPadPos.x >= -0.4 && rightTrackPadPos.y <= 0.4 && rightTrackPadPos.y  >= -0.4){  // 오른쪽 컨트롤러 트랙패드 중앙 누를 시
                    PauseGame();
                }
        }

        if(gState == GameState.Pause){
            
              if(trackPad.GetStateDown(rightHand)){
                rightTrackPadPos = trackPadPosition.GetAxis(any); 

                if(rightTrackPadPos.y >= 0.6 && rightTrackPadPos.x <= 0.4 && rightTrackPadPos.x  >= -0.4){  // 오른쪽 컨트롤러 트랙패드 중앙 누를 시
                    PauseGame();
                }

                else if(rightTrackPadPos.x >= 0.6 && rightTrackPadPos.y <= 0.4 && rightTrackPadPos.y  >= -0.4){ // 게임 재시작 /  트랙패드 오른쪽, 중앙
                    RestartGame();
                }
                else if(rightTrackPadPos.y <= -0.6 && rightTrackPadPos.x <= 0.4 && rightTrackPadPos.x  >= -0.4){ // 게임 실행 파일 종료 / 트랙패드 중하단
                    QuitGame();
                }
            }
        }

        // 게임 오버/승리  상태에서 트랙패드 왼쪽 = 재시작  오른쪽 = 실행종료
        if(gState == GameState.GameOver){
             if(trackPad.GetStateDown(rightHand)){
                rightTrackPadPos = trackPadPosition.GetAxis(any); 
                if(rightTrackPadPos.x <= -0.6 && rightTrackPadPos.y <= 0.4 && rightTrackPadPos.y  >= -0.4){ // 게임 재시작 /  트랙패드 왼쪽, 중앙
                    RestartGame();
                }
                else if(rightTrackPadPos.x >= 0.6 && rightTrackPadPos.y <= 0.4 && rightTrackPadPos.y  >= -0.4){ // 게임 실행 파일 종료 / 트랙패드 오른쪽, 중앙
                    QuitGame();
                }
            }
        }




        // 남은 시간 감소
        if (timeStart == true)    // flag 새로만들기
        {
            LimitTime -= Time.deltaTime;
            text_timer.text = (Mathf.Round(LimitTime)).ToString();
            text_timer_vr.text = (Mathf.Round(LimitTime)).ToString();
        }

        // 제한 시간 경과 후 플레이어 생존 상태이면서 획득한 콜라가 7개 이상
        if(LimitTime <= 0 && PlayerColaCount >= 7)
        {
            // 플레이어의 애니메이션을 멈춘다
           // player.GetComponentInChildren<Animator>().SetFloat("MoveMotion", 0f);

            //상태 텍스트 활성화
            gameOverUI.SetActive(true);

            // 플레이어 승리로 내용변경
            gameOverWinText.text = "Player Win";

            // 상태 텍스트 색상
            gameOverWinText.color = new Color32(255, 228, 0, 255);


            // 상태 텍스트의 자식 오브젝트의 트팬스폼 컴포넌트를 가져오기
            Transform buttons = gameText.transform.GetChild(0);

            // 버튼 오브젝트 활성화 / 게임 오버 되었을 때의 restart 와 quit 버튼
           // buttons.gameObject.SetActive(true);



            // 상태를 '게임 오버' 상태로 변경
            gState = GameState.GameOver;

            // 마우스 커서 활성화
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 타이머 중지
            timeStart = false;

            //  씬 중지
            Time.timeScale = 0f; 

        }

        // 감도 조절
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            print("감도 down");
            
            PlayerRotate.PlayerRotSpeed -= 3;
            CamRotate.CamRotSpeed -= 3;

            mouseSensitivity--;
            text_MouseSensitivity.text = mouseSensitivity.ToString();
        }
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            print("감도 up");

            PlayerRotate.PlayerRotSpeed +=3;
            CamRotate.CamRotSpeed += 3;

            mouseSensitivity++;
            text_MouseSensitivity.text = mouseSensitivity.ToString();
        }



        // T키로 시점 변환하기  / 기본 시점은 FPS
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isFps)
            {
                CamPosition.transform.position = TPS_pos.transform.position;
                isFps = false;
            }
            else
            {
                CamPosition.transform.position = FPS_pos.transform.position;
                isFps = true;
            }
        }


    }

    IEnumerator ReadyToStart()
    {
        // 2초간 대기
        yield return new WaitForSeconds(2f);

        // 상태 텍스트의 내용을 'Go!' 로 한다
        gameText.text = "Go!";

        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 상태 텍스트 비활성화
        gameLabel.SetActive(false);

        // 상태를 '게임 중' 상태로 변경
        gState = GameState.Run;

        timeStart = true;
        
    }

    // 옵션 화면 키기
    public void OpenOptionWindow()
    {
        // 타이머 중지
        timeStart = false;

        // 옵션 창 활성화
        gameOption.SetActive(true);

        // 게임 속도 0배속으로전환
        Time.timeScale = 0f;

        // 게임 상태를 '일시 정지' 상태로 변경
        gState = GameState.Pause;

        // 마우스 커서 활성화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 계속하기 옵션 / Resume
    public void CloseOptionWindow()
    {
        // 타이머 재개
        timeStart = true;

        // 옵션 창 비활성화
        gameOption.SetActive(false);

        // 게임 속도 기존의 1배속으로 전환
        Time.timeScale = 1f;

        // 게임 상태를 '게임 중' 상태로 변경
        gState = GameState.Run;

        // 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 다시 시작하기 옵션
    public void RestartGame()
    {
        // 게임 속도 1배속으로 전환
        Time.timeScale = 1f;

        // 현재 씬 번호 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 게임 종료 옵션
    public void QuitGame()
    {
        // 애플리케이션 종료
        Application.Quit();
    }


    // 적 생성 코루틴 함수
    IEnumerator CreateEnemy()
    {

        while(gState == GameState.Run) // 게임 실행 중일때만 적 생성
        {
          
            yield return new WaitForSeconds(respawnTime);

            // 정해둔 위치에서 랜덤으로 생성
            int idx = Random.Range(0, respawnPoints.Length);

            // 적군 동적 생성
            Instantiate(EnemyPrefab, respawnPoints[idx].position, respawnPoints[idx].rotation);
            if(idx == 0)
            {
                Instantiate(EnemyPrefab, respawnPoints[idx + 1].position, respawnPoints[idx + 1].rotation);
            }
            else
            {
                Instantiate(EnemyPrefab, respawnPoints[idx - 1].position, respawnPoints[idx - 1].rotation);
            }

            
        }
        
    }

    // 보스 타입 적군 생성
    IEnumerator CreateBossEnemy()
    {

        while (gState == GameState.Run) // 게임 실행 중일때만 적 생성
        {
            yield return new WaitForSeconds(respawnTime_Boss);

            // 정해둔 위치에서 랜덤으로 생성
            int idx = Random.Range(0, respawnPoints.Length);

            // 적군 동적 생성
            Instantiate(BossEnemyPrefab, respawnPoints[idx].position, respawnPoints[idx].rotation);
            if (idx == 0)
            {
                Instantiate(BossEnemyPrefab, respawnPoints[idx + 1].position, respawnPoints[idx + 1].rotation);
            }
            else
            {
                Instantiate(BossEnemyPrefab, respawnPoints[idx - 1].position, respawnPoints[idx - 1].rotation);
            }

        }

    }

    // 콜라 생성 코루틴
    IEnumerator CreateCola()
    {
        while (gState == GameState.Run)
        {
            if (shuffleCola)
            {
                // 콜라 리스폰 지역 랜덤으로 섞기 위함
                for (int i = 0; i < ColaRespawnPoints.Length; i++)
                {
                    int ranNum = Random.Range(0, ColaRespawnPoints.Length);
                    Transform temp = ColaRespawnPoints[i];

                    ColaRespawnPoints[i] = ColaRespawnPoints[ranNum];
                    ColaRespawnPoints[ranNum] = temp;
                }

                print("cola shuffle");

                shuffleCola = false;
            }

            yield return new WaitForSeconds(colaRespawnTime);

            if (currentCola < maxCola)
            {
                Instantiate(ColaPrefab, ColaRespawnPoints[currentCola].position, ColaRespawnPoints[currentCola].rotation);

                currentCola++;

                print("cola " + currentCola + " instantiate");
            }

        }

    }

    // 플레이어 사망 시  사망 애니메이션 보여주는 시간만큼 지연   // 피격 사운드 and 사망 사운드 따로??
    IEnumerator PlayerDeadDelay()
    {
        yield return new WaitForSeconds(4.5f);

        // 상태를 '게임 오버' 상태로 변경
        gState = GameState.GameOver;

        // 마우스 커서 활성화
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 타이머 중지
        timeStart = false;

        //  씬 중지
        Time.timeScale = 0f;
       
    }

    public void PauseGame(){  // 게임 일시정지
         if (gState == GameState.Run)
            {
                OpenOptionWindow();
            }
            else if(gState == GameState.Pause)
            {
                CloseOptionWindow();
            }
    }
    
}
