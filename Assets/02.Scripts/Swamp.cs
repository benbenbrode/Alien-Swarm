using System.Collections;
using UnityEngine;

public class Swamp : MonoBehaviour
{
    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        // ������ ������Ʈ�� "Player" �±׸� ������ �ִ��� Ȯ��
        if (other.CompareTag("Player"))
        {
            // PlayerCtrl ��ũ��Ʈ�� �����ͼ� �ڷ�ƾ�� ����
            PlayerCtrl playerCtrl = other.GetComponent<PlayerCtrl>();
            if (playerCtrl != null)
            {
                damageCoroutine = StartCoroutine(DamagePlayerCoroutine(playerCtrl));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // "Player" �±׸� ���� ������Ʈ�� ������ ����� �� �ڷ�ƾ�� ����
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamagePlayerCoroutine(PlayerCtrl playerCtrl)
    {
        while (true)
        {
            if (0.0 > playerCtrl.m_SdOnTime)
            {
                playerCtrl.hp -= 20; // hp ���� -10 ����
                playerCtrl.hpui();   // hpUI �޼��� ����
                if (playerCtrl.hp <= 0)
                {
                    playerCtrl.PlayerDie();
                }
            }
            yield return new WaitForSeconds(1f); // 1�� ���
        }
    }
}
