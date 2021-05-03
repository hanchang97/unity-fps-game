using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public int damage = 5;  // 총알 데미지
    public Gun gunS;
    public GameObject GunMoedl;

    void Start(){
        GunMoedl = GameObject.Find("Gun");
        gunS = GunMoedl.GetComponent<Gun>();
    }   

    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision) {
       
       if(collision.gameObject.tag == "Enemy"){

           // 레
            EnemyFSM eFSM = collision.transform.GetComponent<EnemyFSM>();
             // eFSM.hp -= weaponPower; // 적군 체력 감소

            eFSM.HitEnemy(damage);  // 권총 한 발에 데미지5

           Destroy(gameObject);  // 땅에 닿고 3초뒤 사라진다
       }
       else{ // 적 외 다른 곳

            gunS.bulletEffect.transform.position = collision.transform.position;
            ContactPoint ct = collision.contacts[0];
            gunS.bulletEffect.transform.forward = ct.normal;
            gunS.ps.Play();

           Destroy(gameObject);
       }

      
   }
}
