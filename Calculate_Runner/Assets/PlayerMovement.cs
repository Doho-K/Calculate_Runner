using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 좌우 이동 속도
    public float forwardSpeed = 3f; // 앞으로 전진 속도
    private Rigidbody rb;

    void Start()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
{
    // 수평 입력 (좌우 이동)
    float horizontalInput = Input.GetAxis("Horizontal");

    // 현재 위치를 기준으로 새로운 위치 계산
    Vector3 newPosition = transform.position;

    // 좌우 이동 계산 (좌표로 바로 설정)
    newPosition.x += horizontalInput * moveSpeed * Time.fixedDeltaTime;

    // z축으로 전진
    newPosition.z += forwardSpeed * Time.fixedDeltaTime;

    // x축 위치를 26~34 사이로 제한
    newPosition.x = Mathf.Clamp(newPosition.x, 26, 34);

    // 계산된 위치로 이동
    transform.position = newPosition;
}

    void OnTriggerEnter(Collider other)
    {
        // 충돌 감지
        if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHit(other.gameObject);
            }
        Debug.Log($"Triggered by: {other.gameObject.name}");
    }
}
