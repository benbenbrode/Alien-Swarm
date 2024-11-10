using UnityEngine;
using UnityEngine.UI;

public class Volum : MonoBehaviour
{
    public Slider volumeSlider; // 볼륨 조절 슬라이더
    private const string VolumePrefKey = "VolumePref"; // PlayerPrefs 키

    void Start()
    {
        // 저장된 볼륨 값 불러오기 (저장된 값이 없으면 기본값 1로 설정)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;

        // 슬라이더 값이 변경될 때마다 OnVolumeChange 메서드를 호출하도록 설정
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
    }

    // 슬라이더 값이 변경될 때 호출되는 메서드
    void OnVolumeChange(float value)
    {
        AudioListener.volume = value; // 전체 볼륨을 슬라이더 값으로 설정
        PlayerPrefs.SetFloat(VolumePrefKey, value); // 현재 볼륨 값을 저장
    }
}
