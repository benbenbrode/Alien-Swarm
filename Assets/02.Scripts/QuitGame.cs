using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // ���� ���� �޼���
    public void Quit()
    {
#if UNITY_EDITOR
        // ������ ��忡�� ���� (�����Ϳ����� Application.Quit()�� �۵����� �����Ƿ� ���)
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ����� ���ø����̼ǿ��� ����
            Application.Quit();
#endif
    }
}
