using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 수류탄 투척 및 총알 / 총알은 레이캐스트로 구현
/// </summary>

public class PlayerFire : MonoBehaviour
{
    // 발사위치 
    public GameObject firePosition;

    // 투척 무기 오브젝트
    public GameObject bombFactory;

    // 투척 세기
    public float throwPower = 15f;

    // 총알 피격 이펙트 오브젝트
    public GameObject bulletEffect;

    // 피격 이펙트 파티클 시스템
    ParticleSystem ps;

    //  적군 피격 이펙트 오브젝트
    public GameObject enemyBloodEffect;

    // 적군 피격시 혈흔 파티클 시스템

    ParticleSystem ps2;

    // 발사 무기 공격력
    int weaponPower = 3;

    // 애니메이터 변수
    Animator anim;

    // 무기 모드 변수
    enum WeaponMode
    {
        Normal,  // AR
        Sniper   // DMR
    }

    // 무기 모드
    WeaponMode wMode;

    // 스나이퍼 모드 시 카메라 확대 확인용 변수
    bool ZoomMode = false;

    // 무기 모드 텍스트
    public Text wModeText;


    // 총구 발사 효과 오브젝트 배열
    public GameObject[] eff_Flash;

    // rifle 발사 사운드
    AudioSource aSource;

    // 사운드 클립 배열
    public List<AudioClip> aClipList = new List<AudioClip>();

    // 총알 발사 오디오 클립(연사)
    public AudioClip shootSound;

    // 수류탄 폭발 오디오 클립
    public AudioClip grenadeExplosionSound;

    // 스나이퍼 발사 오디오 클립
    public AudioClip sniperShootSound;

    // 장전 사운드
    public AudioClip reloadSound;

    // 무기 교체 사운드
    public AudioClip weaponChangeSound;


    // 총알 발사 간격  /  연사 시 필요
    float timeBetFire = 0.095f;

    // 마지막으로 rifle 발사한 시점
    float lastRifleFireTime;

    // 스나이퍼 모드에서 발사 간격
    float sniperTimeBetFire = 1.0f;

    // 마지막으로 스나이퍼 발사한 시점
    float lastSniperFireTime;

    // 스나이퍼 발사 가능 상태
    bool sniperAvailable = true;

    // 무기 아이콘 스프라이트 변수
    public GameObject weapon1;
    public GameObject weapon2;

    // 크로스헤어 스프라이트 변수
    public GameObject crosshair1;
    public GameObject crosshair2;

    // 우클릭 모드에 따른 아이콘 스프라이트 변수
    public GameObject weapon1_R;
    public GameObject weapon2_R;

    // 스나이퍼 모드에서 마우스 우클릭 시 zoom 모드 스프라이트 변수
    public GameObject crosshair2_zoom;


    // 현재 탄창에서 남은 총알 수 / rifle
    int bulletCurrent_rifle;

    // 한 탄창당 최대 총알 수 /  rifle
    int bullet_rifle = 30;

    // 총 총알 수 / rifle
    int bulletTotal_rifle = 150;

    // 현재 탄창에서 남은 총알 수 / sniper
    int bulletCurrent_sniper;

    // 한 탄창당 최대 총알 수 / sniper
    int bullet_sniper = 10;

    // 총 총알 수 / sniper
    int bulletTotal_sniper = 30;

    // 남은 총알 수 텍스트
    public Text text_bulletCurrent;

    // 한 탄창당 최대 총알 수 텍스트
    public Text text_bullet;

    // 총 총알 수 텍스트
    public Text text_bulletTotal;

    // 현재 수류탄 개수
    int grenade_current;

    // 최대 소지 가능 수류탄 개수
    int grenade_max = 5;

    // 수류탄 현재 개수 정보 텍스트
    public Text text_grenade_current;

    // 수류탄 최대 소지 가능 개수 정보 텍스트
    public Text text_grenade_max;


    // 플레이어 정보를 받아오기 위한 변수
    Transform player;


    // 장전중
    public bool isReloading = false;
    public bool isReloading2 = false;

    // 수류탄 던지는 중
    public bool isGrenade = false;


    // 연사모드 총 모델
    public GameObject RifleModel;

    // 저격모드 총 모델
    public GameObject SniperModel;

    // 아이템 획득 사운드를 FirePosition 오브젝트에 위치
    public GameObject FirePos;

    public AudioClip getColaSound;

    public AudioClip getItemSound;

    AudioSource itemAudioSource;



    







    // Start is called before the first frame update
    void Start()
    {
        // 총알 피격 이펙트 오브젝트에서 파티클 시스템 컴포넌트 가져오기
        ps = bulletEffect.GetComponent<ParticleSystem>();

        // 적군 피격 시 혈흔 이펙트 오브젝트에서 파티클 시스템 컴포넌트 가져오기
        ps2 = enemyBloodEffect.GetComponent<ParticleSystem>();

        // 애니메이터 컴포넌트 가져오기
        anim = GetComponentInChildren<Animator>();

        // 초기 기본 무기 모드
        wMode = WeaponMode.Normal;

        wModeText.text = "Rifle&Grenade";

        // 오디오 소스 컴포넌트 가져오기
        aSource = GetComponent<AudioSource>();

        itemAudioSource = FirePos.GetComponent<AudioSource>();



        // 오디오 클립 리스트에 추가
        aClipList.Add(shootSound);
        aClipList.Add(grenadeExplosionSound);
        aClipList.Add(sniperShootSound);
        aClipList.Add(reloadSound);
        aClipList.Add(weaponChangeSound);

        // 시작 시 현재 탄창 총알 수 및 수류탄 개수
        bulletCurrent_rifle = bullet_rifle;
        bulletCurrent_sniper = bullet_sniper;
        grenade_current = grenade_max;

        // 플레이어 정보 위함
        player = GameObject.Find("Player").transform;

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Rifle")   // rifle 총알 아이템
        {
            bulletTotal_rifle += 30;

            itemAudioSource.clip = getItemSound;
            itemAudioSource.Play();

            if (wMode == WeaponMode.Normal)
            {
                text_bulletTotal.text = bulletTotal_rifle.ToString();
            }

            Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Grenade")   // 수류탄 아이템
        {
            itemAudioSource.clip = getItemSound;
            itemAudioSource.Play();

            if (grenade_current < 5) 
            {
                grenade_current++;
            }

            text_grenade_current.text = grenade_current.ToString();
            Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Sniper")   // 저격 총알 아이템
        {
            bulletTotal_sniper += 5;

            itemAudioSource.clip = getItemSound;
            itemAudioSource.Play();

            if (wMode == WeaponMode.Sniper)
            {
                text_bulletTotal.text = bulletTotal_sniper.ToString();
            }
            Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Hp")// 체력 아이템
        {

            itemAudioSource.clip = getItemSound;
            itemAudioSource.Play();

            if (player.GetComponent<PlayerMove>().hp <= 15) 
            {
                player.GetComponent<PlayerMove>().hp += 5;
            }
            else
            {
                player.GetComponent<PlayerMove>().hp = 20;
            }
            Destroy(hit.gameObject);
        }
        if(hit.gameObject.tag == "Cola")
        {
            itemAudioSource.clip = getColaSound;
            itemAudioSource.Play();

            GameManager.PlayerColaCount++;  // 획득 콜라 개수 증가
            Destroy(hit.gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {
        // 게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다.
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        

        if(wMode == WeaponMode.Sniper)
        {
            // 발사 후 설정한 시간 지나면 다시 스나이퍼 발사 가능 상태로 전환
            if (Time.time >= lastSniperFireTime + sniperTimeBetFire)
            {
                sniperAvailable = true;

                if (!ZoomMode)
                {
                    crosshair2.SetActive(true);  // 스나이퍼 기본 모드 시 크로스헤어 다시 생성
                }
            }
        } 

       


        // normal mode : 우클릭 시 시선 방향으로 수류탄 투척
        // sniper mode : 우클릭 시 화면 확대 스나이퍼

        //마우스 우클릭 시 시선 방향으로 수류탄 투척  / 우클릭은 1번

        // 마우스 우클릭 입력 받기
        if (Input.GetMouseButtonDown(1))
        {
            switch (wMode)  //무기 모드에 따른 우클릭 행동 변화
            {
                case WeaponMode.Normal:

                    if (grenade_current > 0 && !isGrenade)
                    {
                        isGrenade = true;

                        // 수류탄 투척 애니메이션 실행
                        anim.SetTrigger("Grenade");

                        // 수류탄 오브젝트 생성 후 수류탄 생성 위치를 발사 위치로 한다
                        /* GameObject bomb = Instantiate(bombFactory);
                        bomb.transform.position = firePosition.transform.position;

                        // 수류탄 오브젝트의 Rigidbody 컴포넌트 가져오기
                        Rigidbody rb = bomb.GetComponent<Rigidbody>(); */

                        // 애니메이션과 맞추기 위해 수류탄 나가는 시점 조금 지연
                        StartCoroutine(GrenadeDelay());

                        // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다 / Impulse = 순간적인 힘을 가한다, 질량의 영향 받는다
                        // rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);

                        // 수류탄 개수 감소
                        grenade_current--;
                        text_grenade_current.text = grenade_current.ToString();
                    }

                    break;

                case WeaponMode.Sniper:
                    // 줌 모드 상태가 아니면 우클릭 시 카메라를 확대하고 줌 모드 상태로 변경한다
                    if (!ZoomMode)
                    {
                        if (sniperAvailable) // 스나이퍼 발사 후 다시 발사 가능 상태가 되었을때 줌 가능
                        {
                            Camera.main.fieldOfView = 10f;  // 메인카메라 시야각을 15로 줄이면서 확대
                            ZoomMode = true;

                            // 줌 모드에서 크로스헤어 변경
                            crosshair2_zoom.SetActive(true);
                            crosshair2.SetActive(false);
                        }
                    }
                    else  // 카메라를 원래 상태로 돌리고 줌 모드 해제
                    {
                        Camera.main.fieldOfView = 60f;
                        ZoomMode = false;

                        // 줌 모드 풀면 크로스헤어 변경
                        crosshair2_zoom.SetActive(false);
                        crosshair2.SetActive(true);
                    }
                    break;
            }

           
        }


        // 마우스 좌클릭 시 시선 방향으로 총 발사

        // 마우스 좌클립 입력 받기
        if (Input.GetMouseButton(0))
        {
            if (wMode == WeaponMode.Normal && !isReloading)  // 노말 모드 사용 시
            {
                if (Time.time >= lastRifleFireTime + timeBetFire && bulletCurrent_rifle > 0)
                {
                    // 총알 소비
                    bulletCurrent_rifle--;

                    // 연사 모드에서는 공격력 3 / 스나이퍼는 5
                    weaponPower = 3;

                    lastRifleFireTime = Time.time;

                    // 만일 이동 블렌드 트리 파라미터의 값이 0이라면, 공격 애니메이션 실시
                   /* if (anim.GetFloat("MoveMotion") == 0)
                    {
                        //anim.SetTrigger("Attack");
                        anim.SetTrigger("Shoot");
                    }
                    */

                    

                    // 레이를 생성한 후 발사될 위치와 진행 방향을 설정
                    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

                    // 레이가 부딪힌 대상의 정보를 저장 할 변수 생성 / RaycasHit 구조체
                    RaycastHit hitInfo = new RaycastHit();

                    // 레이를 발사한 후 부딪힌 물체가 있다면 피격 이펙트 표시
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        //레이는 눈에 보이지 않으므로 레이가 닿은 위치에 이펙트 표시

                        if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                        {
                            // 레이에 충돌한 대상의 레이어가 'Enemy'라면 EnemyFSM 컴포넌트를 가져와서 데미지 실행 함수를 실행
                            EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                            // eFSM.hp -= weaponPower; // 적군 체력 감소

                            enemyBloodEffect.transform.position = hitInfo.point;

                            enemyBloodEffect.transform.forward = hitInfo.normal;

                            ps2.Play();

                            eFSM.HitEnemy(weaponPower);
                        }
                        else // 그렇지 않다면 레이에 부딪힌 지점에 피격 이펙트 플레이
                        {
                            //피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킴
                            bulletEffect.transform.position = hitInfo.point;

                            //피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킴 / 피격 이펙트 생성 방향이 자연스럽게 보이기 위해
                            bulletEffect.transform.forward = hitInfo.normal;

                            // 피격 이펙트 플레이
                            ps.Play();
                        }
                    }

                    // 총기 발사 사운드 플레이
                    aSource.clip = aClipList[0];
                    aSource.Play();


                    // 총구 이펙트 실행
                    StartCoroutine(ShootEffectOn(0.05f));

                    // 총알 텍스트
                    text_bulletCurrent.text = bulletCurrent_rifle.ToString();
                    text_bulletTotal.text = bulletTotal_rifle.ToString();
                }
            }
            else if(wMode == WeaponMode.Sniper && !isReloading2)  // 스나이퍼 발사 후에는 줌 모드 풀리고 다음 발사까지 1초 정도 간격 두기
            {
                if (Time.time >= lastSniperFireTime + sniperTimeBetFire && bulletCurrent_sniper > 0)
                {
                    // 총알 소비
                    bulletCurrent_sniper--;

                    // 스나이퍼 모드 선택 시 공격력 상승  // 현재 적군 최대 체력 = 15
                    weaponPower = 9;

                    sniperAvailable = false;

                    lastSniperFireTime = Time.time;

                    // 만일 이동 블렌드 트리 파라미터의 값이 0이라면, 공격 애니메이션 실시
                    if (anim.GetFloat("MoveMotion") == 0)
                    {
                        anim.SetTrigger("Shoot");
                    }

                    // 레이를 생성한 후 발사될 위치와 진행 방향을 설정
                    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

                    // 레이가 부딪힌 대상의 정보를 저장 할 변수 생성 / RaycasHit 구조체
                    RaycastHit hitInfo = new RaycastHit();

                    // 레이를 발사한 후 부딪힌 물체가 있다면 피격 이펙트 표시
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        //레이는 눈에 보이지 않으므로 레이가 닿은 위치에 이펙트 표시

                        if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                        {
                            // 레이에 충돌한 대상의 레이어가 'Enemy'라면 EnemyFSM 컴포넌트를 가져와서 데미지 실행 함수를 실행
                            EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                            // eFSM.hp -= weaponPower; // 적군 체력 감소

                            eFSM.HitEnemy(weaponPower);
                        }
                        else // 그렇지 않다면 레이에 부딪힌 지점에 피격 이펙트 플레이
                        {
                            //피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동시킴
                            bulletEffect.transform.position = hitInfo.point;

                            //피격 이펙트의 forward 방향을 레이가 부딪힌 지점의 법선 벡터와 일치시킴 / 피격 이펙트 생성 방향이 자연스럽게 보이기 위해
                            bulletEffect.transform.forward = hitInfo.normal;

                            // 피격 이펙트 플레이
                            ps.Play();
                        }
                    }

                    // 총기 발사 사운드 플레이
                    aSource.clip = aClipList[2];
                    aSource.Play();


                    // 총구 이펙트 실행
                    StartCoroutine(ShootEffectOn(0.05f));


                    // 줌모드 였다면 풀림
                    if (ZoomMode)
                    {
                        Camera.main.fieldOfView = 60f;
                        ZoomMode = false;
                        crosshair2_zoom.SetActive(false);
                    }

                    // 발사 후 지연 시간 동안은 스나이퍼 크로스헤어 사라짐
                    crosshair2.SetActive(false);

                    // 총알 텍스트
                    text_bulletCurrent.text = bulletCurrent_sniper.ToString();
                    text_bulletTotal.text = bulletTotal_sniper.ToString();
                }
               
            }
        }


        // 키보드 숫자 1번 입력 시 일반 무기 모드
        if (Input.GetKeyDown(KeyCode.Alpha1) && wMode == WeaponMode.Sniper)
        {
            wMode = WeaponMode.Normal;

            anim.SetTrigger("ChangeWeapon");
            aSource.clip = aClipList[4]; // 무기 교체 사운드
            aSource.Play();

            // 총 모델 교체
            RifleModel.SetActive(true);
            SniperModel.SetActive(false);

            // 카메라 시야 원래대로
            Camera.main.fieldOfView = 60f;

            // 일반 무기 모드 텍스트 출력
            wModeText.text = "Rifle&Grenade";

            // 무기, 크로스헤어 1번 스프라이트 활성화, 2번은 비활성화
            weapon1.SetActive(true);
            weapon2.SetActive(false);
            crosshair1.SetActive(true);
            crosshair2.SetActive(false);
            weapon1_R.SetActive(true);
            weapon2_R.SetActive(false);
            text_grenade_current.gameObject.SetActive(true);
            text_grenade_max.gameObject.SetActive(true);

            // rifle 총알 정보
            text_bullet.text = "/ " + bullet_rifle.ToString();
            text_bulletCurrent.text = bulletCurrent_rifle.ToString();
            text_bulletTotal.text = bulletTotal_rifle.ToString();


            // 스나이퍼 줌모드에서 rifle모드로 변경할 수도 있다
            crosshair2_zoom.SetActive(false);
            ZoomMode = false;
        }

        // 키보드 숫자 2번 입력 시 스나이퍼 모드
        if (Input.GetKeyDown(KeyCode.Alpha2) && wMode == WeaponMode.Normal)
        {
            wMode = WeaponMode.Sniper;

            anim.SetTrigger("ChangeWeapon"); // 무기 교체 애니메이션
            aSource.clip = aClipList[4]; // 무기 교체 사운드
            aSource.Play();

            // 총 모델 교체
            RifleModel.SetActive(false);
            SniperModel.SetActive(true);

            // 스나이퍼 모드 텍스트 출력
            wModeText.text = "Sniper Mode";

            // 무기, 크로스헤어 2번 스프라이트 활성화, 1번은 비활성화
            weapon1.SetActive(false);
            weapon2.SetActive(true);
            crosshair1.SetActive(false);
            crosshair2.SetActive(true);
            weapon1_R.SetActive(false);
            weapon2_R.SetActive(true);
            text_grenade_current.gameObject.SetActive(false);
            text_grenade_max.gameObject.SetActive(false);

            // rifle 총알 정보
            text_bullet.text = "/ " + bullet_sniper.ToString();
            text_bulletCurrent.text = bulletCurrent_sniper.ToString();
            text_bulletTotal.text = bulletTotal_sniper.ToString();

        }

        // 키보드 R키 입력 시 재장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (wMode == WeaponMode.Normal)
            {
                if (bulletTotal_rifle > 0 && bulletCurrent_rifle < bullet_rifle)
                {

                    isReloading = true;

                    // 장전 상태 코루틴 함수 호출
                    StartCoroutine(ReloadDelayRifle());

                    // 재장전 애니메이션
                    anim.SetTrigger("Reload");

                    /*if ((bullet_rifle - bulletCurrent_rifle) <= bulletTotal_rifle)
                    {
                        bulletTotal_rifle -= (bullet_rifle - bulletCurrent_rifle);
                        bulletCurrent_rifle += (bullet_rifle - bulletCurrent_rifle);

                    }
                    else
                    {
                        bulletCurrent_rifle += bulletTotal_rifle;
                        bulletTotal_rifle = 0;
                    }*/
                }
                print("총 총알 수: " + bulletTotal_rifle.ToString());

                // 장전 후 총알 정보 업데이트
                /*text_bulletTotal.text = bulletTotal_rifle.ToString();
                text_bulletCurrent.text = bulletCurrent_rifle.ToString(); */

            }
            else if(wMode == WeaponMode.Sniper)
            {
                if (bulletTotal_sniper > 0 && bulletCurrent_sniper < bullet_sniper)
                {

                    isReloading2 = true;

                    // 장전 상태 코루틴 함수 호출
                    StartCoroutine(ReloadDelaySniper());

                    // 재장전 애니메이션
                    anim.SetTrigger("Reload");

                    /*if ((bullet_sniper - bulletCurrent_sniper) <= bulletTotal_sniper)
                    {
                        bulletTotal_sniper -= (bullet_sniper - bulletCurrent_sniper);
                        bulletCurrent_sniper += (bullet_sniper - bulletCurrent_sniper);

                    }
                    else
                    {
                        bulletCurrent_sniper += bulletTotal_sniper;
                        bulletTotal_sniper = 0;
                    }*/
                }
                print("총 총알 수: " + bulletTotal_sniper.ToString());

                // 장전 후 총알 정보 업데이트
                /*text_bulletTotal.text = bulletTotal_sniper.ToString();
                text_bulletCurrent.text = bulletCurrent_sniper.ToString(); */

            }
        }

    }

    // 총구 이펙트 코루틴 함수
    IEnumerator ShootEffectOn(float duration)
    {
        // 랜덤하게 숫자를 뽑기
        int num = Random.Range(0, eff_Flash.Length - 1);

        // 이펙트 오브젝트 배열에서 뽑힌 숫자에 해당하는 이펙트 오브젝트를 활성화
        eff_Flash[num].SetActive(true);

        // 지정한 시간만큼 기다림
        yield return new WaitForSeconds(duration);

        // 이펙트 오브젝트를 다시 비활성화
        eff_Flash[num].SetActive(false);
    }


    // 수류탄 폭발 시 사운드 이펙트
    public void GrenadeExplosion()
    {
        aSource.clip = aClipList[1];
        aSource.Play();
    }


    IEnumerator ReloadDelayRifle()
    {
        if (isReloading)
        {

            aSource.clip = aClipList[3];
            aSource.Play();

            yield return new WaitForSeconds(3.3f);

            if ((bullet_rifle - bulletCurrent_rifle) <= bulletTotal_rifle)
            {
                bulletTotal_rifle -= (bullet_rifle - bulletCurrent_rifle);
                bulletCurrent_rifle += (bullet_rifle - bulletCurrent_rifle);

            }
            else
            {
                bulletCurrent_rifle += bulletTotal_rifle;
                bulletTotal_rifle = 0;
            }

            // 장전 후 총알 정보 업데이트
            text_bulletTotal.text = bulletTotal_rifle.ToString();
            text_bulletCurrent.text = bulletCurrent_rifle.ToString();

            isReloading = false;

        }
    }

    IEnumerator ReloadDelaySniper()
    {
        if (isReloading2)
        {

            aSource.clip = aClipList[3];
            aSource.Play();

            yield return new WaitForSeconds(3.3f);

            if ((bullet_sniper - bulletCurrent_sniper) <= bulletTotal_sniper)
            {
                bulletTotal_sniper -= (bullet_sniper - bulletCurrent_sniper);
                bulletCurrent_sniper += (bullet_sniper - bulletCurrent_sniper);

            }
            else
            {
                bulletCurrent_sniper += bulletTotal_sniper;
                bulletTotal_sniper = 0;
            }

            text_bulletTotal.text = bulletTotal_sniper.ToString();
            text_bulletCurrent.text = bulletCurrent_sniper.ToString();

            isReloading2 = false;

        }
    }

    // 수류탄 동작
    IEnumerator GrenadeDelay()
    {
        if (isGrenade)
        {
            yield return new WaitForSeconds(1f);

            // 수류탄 오브젝트 생성 후 수류탄 생성 위치를 발사 위치로 한다
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = firePosition.transform.position;

            // 수류탄 오브젝트의 Rigidbody 컴포넌트 가져오기
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다 / Impulse = 순간적인 힘을 가한다, 질량의 영향 받는다
            rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);

            isGrenade = false;
        }
    }

}
