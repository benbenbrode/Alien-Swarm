using UnityEngine;
using UnityEngine.UI;

public class Volum : MonoBehaviour
{
    public Slider volumeSlider; // ���� ���� �����̴�
    private const string VolumePrefKey = "VolumePref"; // PlayerPrefs Ű

    void Start()
    {
        // ����� ���� �� �ҷ����� (����� ���� ������ �⺻�� 1�� ����)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;

        // �����̴� ���� ����� ������ OnVolumeChange �޼��带 ȣ���ϵ��� ����
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
    }

    // �����̴� ���� ����� �� ȣ��Ǵ� �޼���
    void OnVolumeChange(float value)
    {
        AudioListener.volume = value; // ��ü ������ �����̴� ������ ����
        PlayerPrefs.SetFloat(VolumePrefKey, value); // ���� ���� ���� ����
    }
}
