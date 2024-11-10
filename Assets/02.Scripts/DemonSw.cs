using System.Collections;
using UnityEngine;

public class DemonSw : MonoBehaviour
{
    public GameObject prefabToSpawn;            // ������ ������
    public GameObject[] spawnPoints;            // ���� ���� ��� (GameObject �迭)
    public float spawnInterval = 10.0f;         // ���� �ֱ� (�� ����)

    void Start()
    {
        StartCoroutine(DelayedStartSpawnRoutine());  // 5�� ���� �� ���� ����
    }

    IEnumerator DelayedStartSpawnRoutine()
    {
        yield return new WaitForSeconds(300f);  // 5�� ���
        StartCoroutine(SpawnRoutine());         // SpawnRoutine �ڷ�ƾ ����
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // ���� ������ �� ������ ��ġ ����
            int randomIndex = Random.Range(0, spawnPoints.Length);
            GameObject spawnPoint = spawnPoints[randomIndex];

            // ���õ� ��ġ�� ������ ����
            Instantiate(prefabToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);
            yield return new WaitForSeconds(spawnInterval);  // 10�� ��� (���� �ֱ�)
        }
    }
}
