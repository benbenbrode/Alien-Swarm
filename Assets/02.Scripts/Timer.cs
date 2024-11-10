using UnityEngine;
using UnityEngine.UI;

public class Timer: MonoBehaviour
{
    public Text timerText; // UI Text ������Ʈ�� ����
    private float timeElapsed = 0f; // ��� �ð� ������ ����
    private bool isBossTime = false; // BOSS �ð����� üũ�ϴ� ����

    void Update()
    {
        if (isBossTime) return; // BOSS �ð����� Ÿ�̸Ӹ� ������Ʈ���� ����

        // �� �����Ӹ��� ��� �ð��� ����
        timeElapsed += Time.deltaTime;

        // 10��(600��)�� ����ϸ� BOSS�� ǥ��
        if (timeElapsed >= 480f)
        {
            timerText.text = "BOSS";
            isBossTime = true; // BOSS �ð����� ��ȯ
        }
        else
        {
            // �а� �ʸ� ����Ͽ� "��:��" �������� ����
            int minutes = Mathf.FloorToInt(timeElapsed / 60f);
            int seconds = Mathf.FloorToInt(timeElapsed % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}"; // "��:��" �������� ǥ��
        }
    }
}
