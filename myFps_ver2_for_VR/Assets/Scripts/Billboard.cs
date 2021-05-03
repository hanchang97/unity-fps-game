using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  적군 체력바 오브젝트가 플레이어 카메라를 바라보도록 하기 위함
/// </summary>
public class Billboard : MonoBehaviour
{
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 캔버스 방향을 플레이어 카메라의 방향과 일치 시킨다
        transform.forward = target.forward;
    }
}
