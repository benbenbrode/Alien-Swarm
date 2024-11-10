using UnityEngine;
using UnityEngine.UI;

public class Timer: MonoBehaviour
{
    public Text timerText; // UI Text 컴포넌트를 연결
    private float timeElapsed = 0f; // 경과 시간 추적용 변수
    private bool isBossTime = false; // BOSS 시간인지 체크하는 변수

    void Update()
    {
        if (isBossTime) return; // BOSS 시간에는 타이머를 업데이트하지 않음

        // 매 프레임마다 경과 시간을 증가
        timeElapsed += Time.deltaTime;

        // 10분(600초)이 경과하면 BOSS로 표시
        if (timeElapsed >= 480f)
        {
            timerText.text = "BOSS";
            isBossTime = true; // BOSS 시간으로 전환
        }
        else
        {
            // 분과 초를 계산하여 "분:초" 형식으로 설정
            int minutes = Mathf.FloorToInt(timeElapsed / 60f);
            int seconds = Mathf.FloorToInt(timeElapsed % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}"; // "분:초" 형식으로 표시
        }
    }
}
