using UnityEngine;

public class BossSw : MonoBehaviour
{
    public GameObject bossPrefab; // 생성할 프리팹 (예: 보스 오브젝트)
    public Transform spawnLocation; // 프리팹이 생성될 위치
    private float elapsedTime = 0f; // 경과 시간을 추적하기 위한 변수
    private bool hasSpawned = false; // 한 번만 생성되도록 체크하는 변수

    void Update()
    {
        if (hasSpawned) return; // 이미 생성되었으면 더 이상 실행하지 않음

        elapsedTime += Time.deltaTime; // 프레임마다 경과 시간 증가

        if (elapsedTime >= 480f) // 480초 = 8분
        {
            Instantiate(bossPrefab, spawnLocation.position, spawnLocation.rotation); // 프리팹 생성
            hasSpawned = true; // 생성 후 다시 생성되지 않도록 설정
        }
    }
}
