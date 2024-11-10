using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityControl : MonoBehaviour
{
    public Slider sensitivitySlider;               // 마우스 감도 조절 슬라이더
    private const string SensitivityPrefKey = "MouseSensitivity"; // PlayerPrefs 키

    void Start()
    {
        // 저장된 마우스 감도 값을 불러오기 (기본값 1.0)
        GlobalValue.g_mouse = PlayerPrefs.GetFloat(SensitivityPrefKey, 1.0f);

        // 슬라이더의 최소/최대값과 초기값 설정
        sensitivitySlider.minValue = 0.1f;
        sensitivitySlider.maxValue = 2.0f;
        sensitivitySlider.value = GlobalValue.g_mouse;

        // 슬라이더 값이 변경될 때마다 OnSensitivityChange 호출
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChange);
    }

    // 슬라이더 값 변경 시 호출되는 메서드
    void OnSensitivityChange(float value)
    {
        GlobalValue.g_mouse = value;  // 마우스 감도를 GlobalValue에 저장
        PlayerPrefs.SetFloat(SensitivityPrefKey, value);  // 변경된 감도를 PlayerPrefs에 저장
    }
}
