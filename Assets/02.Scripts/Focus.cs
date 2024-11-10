using System.Collections;
using UnityEngine;

public class Focus : MonoBehaviour
{
    public Camera mainCamera;                   // ī�޶� ��ü
    public float rotationDuration = 2f;         // ȸ�� ���� �ð�
    private Vector3 originalCameraPosition;     // ���� ī�޶� ��ġ
    private Quaternion originalCameraRotation;  // ���� ī�޶� ȸ��
    private FollowCam followCam;                // FollowCam ��ũ��Ʈ ����

    void Start()
    {
        // ī�޶� �Ҵ���� ���� ���, ���� ī�޶� �ڵ� �Ҵ�
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // FollowCam ������Ʈ ��������
        followCam = mainCamera.GetComponent<FollowCam>();

        // ���� ī�޶� ��ġ�� ȸ���� ����
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        // ���� ���� (�׽�Ʈ�� ���� Start���� ȣ��)
        FocusOnThisObject();
    }

    public void FocusOnThisObject()
    {
        StartCoroutine(FocusRoutine());
    }

    IEnumerator FocusRoutine()
    {
        // FollowCam ��Ȱ��ȭ
        if (followCam != null)
        {
            followCam.enabled = false;
        }

        // �ð� ����
        Time.timeScale = 0f;

        // ������Ʈ �̸��� ���� �ٸ� ���� ����
        Vector3 focusPosition;

        if (gameObject.name == "Player")
        {
            focusPosition = transform.position + transform.forward * -5f + Vector3.up * 3f;
        }
        else
        {
            focusPosition = transform.position + transform.forward * -7f + Vector3.up * 8f;
        }

        // ī�޶� ��ġ ����
        mainCamera.transform.position = focusPosition;
        mainCamera.transform.LookAt(transform.position + Vector3.up * 2f);

        // ������Ʈ�� �߽����� ī�޶� ȸ��
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            mainCamera.transform.RotateAround(transform.position, Vector3.up, 360 * (Time.unscaledDeltaTime / rotationDuration));
            mainCamera.transform.LookAt(transform.position + Vector3.up * 2f);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // ī�޶� ��ġ�� ȸ�� ������� ����
        mainCamera.transform.position = originalCameraPosition;
        mainCamera.transform.rotation = originalCameraRotation;

        // FollowCam Ȱ��ȭ
        if (followCam != null)
        {
            followCam.enabled = true;
        }

        // �ð� ����
        Time.timeScale = 1f;
    }
}
