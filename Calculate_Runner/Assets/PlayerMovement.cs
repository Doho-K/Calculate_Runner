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
        float horizontalInput = Input.GetAxis("Horizontal");

        // 좌우 이동 방향 설정
        Vector3 moveDirection = new Vector3(horizontalInput, 0, 0) * moveSpeed;
        
        // 현재 위치에 이동 방향과 전진 방향을 적용하여 새 위치 계산
        Vector3 newPosition = transform.position + moveDirection * Time.fixedDeltaTime;
        newPosition.z += forwardSpeed * Time.fixedDeltaTime; // z축으로 전진

        // x축 위치를 22~34 사이로 제한
        newPosition.x = Mathf.Clamp(newPosition.x, 26, 34);

        // 제한된 위치로 이동
        rb.MovePosition(newPosition);
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
