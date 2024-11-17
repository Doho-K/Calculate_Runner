using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoFloat : MonoBehaviour
{
    public float floatSpeed = 1f;  // 위아래 이동 속도
    public float floatRange = 15f; // 이동 범위 (픽셀 단위)

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        // RectTransform 가져오기
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition; // 초기 위치 저장
    }

    void Update()
    {
        // Sin 함수를 사용해 위아래로 이동
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        rectTransform.anchoredPosition = new Vector2(startPosition.x, startPosition.y + offset);
    }
}
