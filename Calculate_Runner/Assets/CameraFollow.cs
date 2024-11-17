using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;       // 플레이어의 Transform
    private Vector3 offset;        // 플레이어와 카메라의 초기 거리 (오프셋)

    void Start()
    {
        // 플레이어와 카메라의 초기 거리 계산
        if (player != null)
        {
            offset = transform.position - player.position;
        }
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            // 목표 위치 계산 (x축 고정)
            Vector3 targetPosition = player.position + offset;
            targetPosition.x = transform.position.x; // x축을 현재 카메라 위치로 고정

            // Lerp를 사용하여 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f); // 0.1f는 부드러운 이동 속도
        }
    }
}
