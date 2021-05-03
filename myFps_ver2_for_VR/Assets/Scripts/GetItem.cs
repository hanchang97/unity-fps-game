using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItem : MonoBehaviour
{


    public GameObject itemAudio;
    public GameObject player;

    AudioSource itemAS;

    public AudioClip getItems;
    public AudioClip getCoke;

    public GameObject GunModel;


    // Start is called before the first frame update
    void Start()
    {
        
         itemAS = itemAudio.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //transform.localPosition = new Vector3(0,0,0); // 상대좌표
    }

     private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Rifle")   // rifle 총알 아이템 // 현재는 권총모드
        {
           Debug.Log("Rifle get");

           itemAS.clip = getItems;
            itemAS.Play();

           Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Grenade")   // 수류탄 아이템
        {
           Debug.Log("Grenade get");

            itemAS.clip = getItems;
            itemAS.Play();

            int gr = GunModel.GetComponent<Gun>().current_Grenade;

            if(gr >= 5){
                gr = 10;
            }
            else{
            gr += 5;
            }

            GunModel.GetComponent<Gun>().current_Grenade = gr;
            GunModel.GetComponent<Gun>().UpdateUI();


           Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Sniper")   // 저격 총알 아이템
        {
            Debug.Log("Sniper get");

             itemAS.clip = getItems;
            itemAS.Play();

            Destroy(hit.gameObject);
        }
        if (hit.gameObject.tag == "Hp")// 체력 아이템
        {
                Debug.Log("Hp get");

                 itemAS.clip = getItems;
            itemAS.Play();

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
         if (hit.gameObject.tag == "gun")// 체력 아이템
        {
                Debug.Log("Gun get");

                 itemAS.clip = getItems;
            itemAS.Play();

            GunModel.GetComponent<Gun>().m_MaxAmmo += 15;


           
           Destroy(hit.gameObject);
        }
        if(hit.gameObject.tag == "Cola")
        {
           Debug.Log("Cola get");

            GameManager.PlayerColaCount++;  // 획득 콜라 개수 증가

              itemAS.clip = getCoke;
            itemAS.Play();

          

           Destroy(hit.gameObject);
        }
    }

}
