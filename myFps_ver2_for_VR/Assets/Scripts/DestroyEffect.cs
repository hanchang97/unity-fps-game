using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 수류탄 폭발 후 폭발 이펙트 제거
/// </summary>

public class DestroyEffect : MonoBehaviour
{
    // 제거될 시간 변수
    public float destroyTime = 1.5f;

    // 경과 시간 측정용 변수
    float currentTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 경과 시간이 제거될 시간 초과시 자기 자신 제거
        if(currentTime > destroyTime)
        {
            Destroy(gameObject);
        }

        // 경과 시간 누적
        currentTime += Time.deltaTime;
    }
}
