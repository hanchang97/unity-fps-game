using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///  카메라가 사용자의 눈처럼 동작하기 위함
/// </summary>

public class CamFollow : MonoBehaviour
{
    //목표가 될 트랜스폼 컴포넌트
    public Transform target;

   
    void Start()
    {
        Camera.main.aspect = 16f / 9f;
    }

    
    void Update()
    {
        // 카메라의 위치를 목표 트랜스폼의 위치에 일치시킴
        transform.position = target.position;
    }
}
