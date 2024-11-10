using System.Collections;
using UnityEngine;

public class DemonSw : MonoBehaviour
{
    public GameObject prefabToSpawn;            // 생성할 프리팹
    public GameObject[] spawnPoints;            // 스폰 지점 목록 (GameObject 배열)
    public float spawnInterval = 10.0f;         // 스폰 주기 (초 단위)

    void Start()
    {
        StartCoroutine(DelayedStartSpawnRoutine());  // 5분 지연 후 스폰 시작
    }

    IEnumerator DelayedStartSpawnRoutine()
    {
        yield return new WaitForSeconds(300f);  // 5분 대기
        StartCoroutine(SpawnRoutine());         // SpawnRoutine 코루틴 시작
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 스폰 지점들 중 랜덤한 위치 선택
            int randomIndex = Random.Range(0, spawnPoints.Length);
            GameObject spawnPoint = spawnPoints[randomIndex];

            // 선택된 위치에 프리팹 생성
            Instantiate(prefabToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);
            yield return new WaitForSeconds(spawnInterval);  // 10초 대기 (스폰 주기)
        }
    }
}
