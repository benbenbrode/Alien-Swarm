using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // 게임 종료 메서드
    public void Quit()
    {
#if UNITY_EDITOR
        // 에디터 모드에서 종료 (에디터에서는 Application.Quit()이 작동하지 않으므로 사용)
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 빌드된 애플리케이션에서 종료
            Application.Quit();
#endif
    }
}
