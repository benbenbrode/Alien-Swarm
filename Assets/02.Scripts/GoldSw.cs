using System.Collections;
using UnityEngine;

public class GoldeSw : MonoBehaviour
{
    public GameObject prefabToSpawn;            // ������ ������
    public GameObject[] spawnPoints;            // ���� ���� ��� (GameObject �迭)
    public float spawnInterval = 30.0f;         // ���� �ֱ� (�� ����)

    void Start()
    {
        StartCoroutine(SpawnRoutine());
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
            yield return new WaitForSeconds(spawnInterval);  // 30�� ���
        }
    }
}
