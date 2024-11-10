using System.Collections;
using UnityEngine;

public class Swamp : MonoBehaviour
{
    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        // 접촉한 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // PlayerCtrl 스크립트를 가져와서 코루틴을 시작
            PlayerCtrl playerCtrl = other.GetComponent<PlayerCtrl>();
            if (playerCtrl != null)
            {
                damageCoroutine = StartCoroutine(DamagePlayerCoroutine(playerCtrl));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // "Player" 태그를 가진 오브젝트가 영역을 벗어났을 때 코루틴을 중지
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
                playerCtrl.hp -= 20; // hp 값을 -10 감소
                playerCtrl.hpui();   // hpUI 메서드 실행
                if (playerCtrl.hp <= 0)
                {
                    playerCtrl.PlayerDie();
                }
            }
            yield return new WaitForSeconds(1f); // 1초 대기
        }
    }
}
