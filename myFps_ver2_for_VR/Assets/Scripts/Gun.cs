using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 총기 발사 및 수류탄 발사
public class Gun : MonoBehaviour
{
    public Animator m_Animator; // 총 애니메이터
    public Transform m_FireTransform; // 총구의 위치를 나타내는 트랜스폼
    public Transform grenadeFireTransform;
    public ParticleSystem m_ShellEjectEffect;  // 탄피 배출 효과 재생기
    public ParticleSystem m_MuzzleFlashEffect; //총구 화염 효과 재생기

    public AudioSource m_GunAudioPlayer; // 총 소리 재생기
    public AudioClip m_ShotClip; // 발사소리
    public AudioClip m_ReloadClip; // 재장전 소리

    public LineRenderer m_BulletLineRenderer;  // 총알 궤적 랜더러
    public GameObject m_ImpactPrefab; // 피탄 위치에 생성할 이펙트/데칼 효과 원본

    public Text m_AmmoText; // 남은 탄환 수 표시할 UI Text

    public int total_Ammo = 150;
    public int m_MaxAmmo = 15; // 최대 탄창 수
    public float m_TimeBetFire = 0.3f;
    public int m_Damage = 5;
    public float m_ReloadTime = 2.0f;  // 재장전 2초 동안  다른 동작x
    public float m_FireDistance = 100f; // 총기 사정 거리

    public enum State {Ready, Empty, Reloading};

    public State m_CurrentState = State.Empty; // 현재 총의 상태

    private float m_LastFireTime; // 총을 마지막으로 발사한 시점
    public int m_CurrentAmmo = 0;


    // 총알 피격 이펙트 오브젝트
    public GameObject bulletEffect;

     // 피격 이펙트 파티클 시스템
    public ParticleSystem ps;


    public GameObject bombFactory; // 수류탄
    public int m_Grenade = 10; // 수류탄 최대 개수
    public int current_Grenade = 10; // 현재 수류탄 개수

    // 무기 타입
    enum WeaponType{
        Gun,
        Grenade
    }

    WeaponType wType;

    //  총, 수류탄 모델
    public GameObject gunModel;
    public GameObject grenadeModel;

    // 투척 세기
    public float throwPower = 15f;

    // 현재 무기 나타내는 Text UI
    public Text currentWeaponText;


//// 총알
    public GameObject bullet;



    // Start is called before the first frame update
    void Start()
    {
         // 게임 시작 시
        m_CurrentState = State.Empty;
        m_LastFireTime = 0; // 마지막으로 총을 쏜 시점을 초기화

        m_BulletLineRenderer.positionCount = 2;  // 라인랜더러가 사용할 정점을 두개로 지정
        m_BulletLineRenderer.enabled = false;

        // 총알 피격 이펙트 오브젝트에서 파티클 시스템 컴포넌트 가져오기
        ps = bulletEffect.GetComponent<ParticleSystem>();
    
        // 최초 타입은 Gun
        wType = WeaponType.Gun;

        UpdateUI(); // UI 갱신
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 발사 처리를 시도하는 함수 
    public void Fire(){

        if(wType == WeaponType.Gun){
        // 총이 준비된 상태이고,  현재 시간 >= 마지막 발사시점 + 연사 간격
            if(m_CurrentState == State.Ready && Time.time >= m_LastFireTime + m_TimeBetFire){
                m_LastFireTime = Time.time;  // 마지막으로 총을 쏜 시점이 현재 시점으로 갱신

                Shot();
                UpdateUI();
            }
        }
        else{
            Shot();
            UpdateUI();
        }
    }


    // 실제 발사 처리를 하는 부분
    private void Shot(){
        // 총 모드
        if(wType == WeaponType.Gun){

            // 총알 직접 구현 ver
            //
             GameObject instantBullet = Instantiate(bullet, m_FireTransform.position, m_FireTransform.rotation);
             Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
             bulletRigid.velocity = m_FireTransform.forward * 50;
            //



            /*
            RaycastHit hitInfo = new RaycastHit(); //레이캐스트 정보를 저장하는, 충돌 정보 컨테이너
            Ray ray = new Ray(m_FireTransform.position, m_FireTransform.forward);

            // 총을 쏴서 총알이 맞은 곳
            Vector3 hitPosition = m_FireTransform.position + m_FireTransform.forward * m_FireDistance;

            // 레이캐스트(시작지점, 방향, 충돌 정보 컨테이너, 사정거리)
            if(Physics.Raycast(ray, out hitInfo)){

                // 대상이 IDamageable 로 가져와지면, 대상의  OnDamage 함수 호출해서 데미지 적용
                // IDamageable target = hit.collider.GetComponent<IDamageable>();

                /*if(target != null){
                    target.OnDamage(m_Damage);
                }*/

                /*
                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                                // 레이에 충돌한 대상의 레이어가 'Enemy'라면 EnemyFSM 컴포넌트를 가져와서 데미지 실행 함수를 실행
                                EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                                // eFSM.hp -= weaponPower; // 적군 체력 감소

                                eFSM.HitEnemy(m_Damage);  // 권총 한 발에 데미지5
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

                //충돌위치
                hitPosition = hitInfo.point;

            } */

            // 발사 이펙트 재생 시작
            StartCoroutine(ShotEffect());

            // 남은 탄환 수 -1
            m_CurrentAmmo--;

            if(m_CurrentAmmo <= 0)
            {
                m_CurrentState = State.Empty;
            }
            
        }

        // 수류탄 모드
        else if(wType == WeaponType.Grenade && current_Grenade >= 1){

             // 수류탄 오브젝트 생성 후 수류탄 생성 위치를 발사 위치로 한다
            GameObject bomb = Instantiate(bombFactory);
            bomb.transform.position = grenadeFireTransform.position;

             // 수류탄 오브젝트의 Rigidbody 컴포넌트 가져오기
            Rigidbody rb = bomb.GetComponent<Rigidbody>();

             // 카메라의 정면 방향으로 수류탄에 물리적인 힘을 가한다 / Impulse = 순간적인 힘을 가한다, 질량의 영향 받는다
            rb.AddForce(grenadeFireTransform.forward * throwPower, ForceMode.Impulse);

            //수류탄 개수 감소
            current_Grenade--;

        }
    } // 수류탄은 총기의 총알에 영향 받지 않게 해야한다


    // 발사 이펙트를 재생하고 총알 궤적을 잠시 그렸다가 끄는 함수
    //public IEnumerator ShotEffect(Vector3 hitPosition)
    public IEnumerator ShotEffect()
    {
        m_Animator.SetTrigger("Fire");  // Fire 트리거 당김

        // 총알 궤적 랜더러를 킨다
       /* m_BulletLineRenderer.enabled = true;
        m_BulletLineRenderer.SetPosition(0, m_FireTransform.position);  // 랜더러 첫 위치는 총구 위치

        m_BulletLineRenderer.SetPosition(1, hitPosition);  // 랜더러 선분 두번째 점은 입력으로 들어온 피탄 위치 */


        // 이펙트 재생
        m_MuzzleFlashEffect.Play(); // 총구 화염 효과 재생
        m_ShellEjectEffect.Play(); // 탄피 배출 효과 재생

        if(m_GunAudioPlayer.clip != m_ShotClip)
        {
            m_GunAudioPlayer.clip = m_ShotClip; // 총 발사 소리를 장전 / mshotclip 아닌 경우만 클립 등록 / 성능 최적화
        }

        m_GunAudioPlayer.Play(); // 총격 소리 재생

        yield return new WaitForSeconds(0.07f);

        // m_BulletLineRenderer.enabled = false;  // 총알 궤적 번쩍 하는 효과
    }

    // 총의 탄약 UI에 남은 탄약수를 갱신해서 띄우기
    public void UpdateUI()
    {
        if(wType == WeaponType.Gun){  //  총 모드
            if(m_CurrentState == State.Empty){
                m_AmmoText.text = "EMPTY";
            }

            else if(m_CurrentState == State.Reloading){
                m_AmmoText.text = "RELOADING";
            }
            else{
                m_AmmoText.text = m_CurrentAmmo.ToString();
            }
        }
        else if(wType == WeaponType.Grenade) // 수류탄 모드
        {
            m_AmmoText.text = current_Grenade.ToString();
        }
    }


    // 재장전 시도
    public void Reload(){
        if(m_CurrentState != State.Reloading){  // 재장전 상태 아닐 때 가능
            StartCoroutine(ReloadRoutine());
        }
    }

    // 실제 재장전처리가 진행되는 곳
    private IEnumerator ReloadRoutine(){
        m_Animator.SetTrigger("ReloadGun"); // Reload 애니메이션 파라미터 
        m_CurrentState = State.Reloading;  // 현재 상태를 재장전 상태로 전환

        m_GunAudioPlayer.clip = m_ReloadClip; // 오디오 소스 클립 재장전 소리로 교체
        m_GunAudioPlayer.Play();

        UpdateUI();

        yield return new WaitForSeconds(m_ReloadTime); // 재장전 시간 만큼 지연

        m_CurrentAmmo = m_MaxAmmo; // 탄약 최대 충전

        m_CurrentState = State.Ready;
        UpdateUI();
    }

    // 무기 교체
    public void WeaponChange(){
        if(wType == WeaponType.Gun)
        {
            wType = WeaponType.Grenade;

            gunModel.SetActive(false);
           
            grenadeModel.SetActive(true);

            currentWeaponText.text = "Grenade";

            UpdateUI(); 
        }
        else if(wType == WeaponType.Grenade)
        {
            wType = WeaponType.Gun;

             gunModel.SetActive(true);

            grenadeModel.SetActive(false);

            currentWeaponText.text = "Gun";

             UpdateUI(); 
        }
        /*else if(wType == WeaponType.Gun
        {
            wType = WeaponType.Grenade;
        }*/   // 다른 무기 추가 시 이어서 작성하기
    }

}
