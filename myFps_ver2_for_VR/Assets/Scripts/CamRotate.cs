using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라 회전
/// </summary>

public class CamRotate : MonoBehaviour
{
    // 회전 속도 변수 - 회전하는 속도를 직접 제어     // 플레이어 rotate 속도와 같아야 한다
    public static float CamRotSpeed = 120f;  // 추후에 static 말고 다른 방식으로 컨트롤 해보기

    // 회전 값 변수
    float mx = 0;
    float my = 0;



    // Start is called before the first frame update
    void Start()
    {
       // CamRotSpeed = 120f;
    }

    // Update is called once per frame
    void Update()
    {
        // 게임 상태가 '게임 중' 상태일 때만 조작할 수 있게 한다.
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

/*
        //사용자의 마우스 입력을 받아 물체를 회전
        // 마우스 입력 받기
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        // 회전 값 변수에 마우스 입력 값만큼 미리 누적
        mx += mouse_X * CamRotSpeed * Time.deltaTime;
        my += mouse_Y * CamRotSpeed * Time.deltaTime;

        // 마우스 상하 이동 회전 변수(my) 값을 -90도 ~ 90도 사이로 제한
        my = Mathf.Clamp(my, -90f, 90f);

        // 회전 방향으로 물체를 회전
        // r = r0 +vt
        transform.eulerAngles = new Vector3(-my, mx, 0);

        
*/

    }
}
