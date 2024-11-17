using UnityEngine;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    public Transform headTransform; // 머리 위치를 나타내는 Transform
    public TMP_Text nameText; // TextMeshPro 텍스트 컴포넌트
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 머리 위 텍스트 위치 오프셋
    public float verticalScreenOffset = 50f; // 화면 상단으로 보정할 픽셀 값

    private Camera mainCamera;

    void Start()
    {
        if (nameText == null || headTransform == null)
        {
            Debug.LogError("Required references are missing!");
            return;
        }

        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 월드 좌표에서 화면 좌표로 변환
        Vector3 worldPosition = headTransform.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // 화면 좌표 보정
        screenPosition.y += verticalScreenOffset;

        // 텍스트 위치 업데이트
        if (screenPosition.z > 0) // 카메라 앞에 있는 경우에만 표시
        {
            nameText.transform.position = screenPosition;
            nameText.enabled = true;
        }
        else
        {
            nameText.enabled = false; // 카메라 뒤에 있으면 숨김
        }
    }
}
