using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityControl : MonoBehaviour
{
    public Slider sensitivitySlider;               // ���콺 ���� ���� �����̴�
    private const string SensitivityPrefKey = "MouseSensitivity"; // PlayerPrefs Ű

    void Start()
    {
        // ����� ���콺 ���� ���� �ҷ����� (�⺻�� 1.0)
        GlobalValue.g_mouse = PlayerPrefs.GetFloat(SensitivityPrefKey, 1.0f);

        // �����̴��� �ּ�/�ִ밪�� �ʱⰪ ����
        sensitivitySlider.minValue = 0.1f;
        sensitivitySlider.maxValue = 2.0f;
        sensitivitySlider.value = GlobalValue.g_mouse;

        // �����̴� ���� ����� ������ OnSensitivityChange ȣ��
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChange);
    }

    // �����̴� �� ���� �� ȣ��Ǵ� �޼���
    void OnSensitivityChange(float value)
    {
        GlobalValue.g_mouse = value;  // ���콺 ������ GlobalValue�� ����
        PlayerPrefs.SetFloat(SensitivityPrefKey, value);  // ����� ������ PlayerPrefs�� ����
    }
}
