using System.Collections;
using UnityEngine;

public class GoldeSw : MonoBehaviour
{
    public GameObject prefabToSpawn;            // 생성할 프리팹
    public GameObject[] spawnPoints;            // 스폰 지점 목록 (GameObject 배열)
    public float spawnInterval = 30.0f;         // 스폰 주기 (초 단위)

    void Start()
    {
        StartCoroutine(SpawnRoutine());
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
            yield return new WaitForSeconds(spawnInterval);  // 30초 대기
        }
    }
}
