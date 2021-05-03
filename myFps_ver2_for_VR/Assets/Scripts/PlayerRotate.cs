using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터의 회전
/// </summary>
public class PlayerRotate : MonoBehaviour
{
    // 회전 속도 변수 - 회전하는 속도를 직접 제어   //  추후에 static 말고 다른 방식으로 컨트롤 해보기
    public static float PlayerRotSpeed = 120f;

    // 회전 값 변수
    float mx = 0;

    public Camera mainCam;


    // Start is called before the first frame update
    void Start()
    {
        PlayerRotSpeed = 120f;
    }

    // Update is called once per frame
    void Update()
    {
        // 게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다.
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

/*   == 키보드 ver ==
        
        //사용자의 마우스 입력을 받아 물체를 회전
        // 마우스 입력 받기
        float mouse_X = Input.GetAxis("Mouse X");

        // 회전 값 변수에 마우스 입력 값만큼 미리 누적
        mx += mouse_X * PlayerRotSpeed * Time.deltaTime;

        //회전 방향으로 물체 회전
        transform.eulerAngles = new Vector3(0, mx, 0);
    
    */

      // vr 메인 카메라 rotate y값 만큼 플레이어도 돌아야함
    transform.rotation = Quaternion.Euler(0, mainCam.transform.rotation.eulerAngles.y, 0);
   // Debug.Log(mainCam.transform.eulerAngles);

    }
}
