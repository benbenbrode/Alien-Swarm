using System.Collections;
using UnityEngine;

public class Focus : MonoBehaviour
{
    public Camera mainCamera;                   // 카메라 객체
    public float rotationDuration = 2f;         // 회전 지속 시간
    private Vector3 originalCameraPosition;     // 원래 카메라 위치
    private Quaternion originalCameraRotation;  // 원래 카메라 회전
    private FollowCam followCam;                // FollowCam 스크립트 참조

    void Start()
    {
        // 카메라가 할당되지 않은 경우, 메인 카메라 자동 할당
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // FollowCam 컴포넌트 가져오기
        followCam = mainCamera.GetComponent<FollowCam>();

        // 원래 카메라 위치와 회전값 저장
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        // 연출 시작 (테스트를 위해 Start에서 호출)
        FocusOnThisObject();
    }

    public void FocusOnThisObject()
    {
        StartCoroutine(FocusRoutine());
    }

    IEnumerator FocusRoutine()
    {
        // FollowCam 비활성화
        if (followCam != null)
        {
            followCam.enabled = false;
        }

        // 시간 정지
        Time.timeScale = 0f;

        // 오브젝트 이름에 따라 다른 연출 설정
        Vector3 focusPosition;

        if (gameObject.name == "Player")
        {
            focusPosition = transform.position + transform.forward * -5f + Vector3.up * 3f;
        }
        else
        {
            focusPosition = transform.position + transform.forward * -7f + Vector3.up * 8f;
        }

        // 카메라 위치 설정
        mainCamera.transform.position = focusPosition;
        mainCamera.transform.LookAt(transform.position + Vector3.up * 2f);

        // 오브젝트를 중심으로 카메라 회전
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            mainCamera.transform.RotateAround(transform.position, Vector3.up, 360 * (Time.unscaledDeltaTime / rotationDuration));
            mainCamera.transform.LookAt(transform.position + Vector3.up * 2f);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // 카메라 위치와 회전 원래대로 복구
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;

        // FollowCam 활성화
        if (followCam != null)
        {
            followCam.enabled = true;
        }

        // 시간 복구
        Time.timeScale = 1f;
    }
}
