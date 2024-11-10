using System.Collections.Generic;
using UnityEngine;

public class AttributeManager : MonoBehaviour
{
    public GameObject panel;                // �θ� �� �ǳ� ������Ʈ
    public List<GameObject> prefabList;     // 12���� ������ ����Ʈ
    public List<Transform> spawnPoints;     // �������� ������ 3���� ��ġ
 

    public void SpawnRandomPrefabs()
    {
        panel.SetActive(true);
        GameObject[] attriObjects = GameObject.FindGameObjectsWithTag("attri");


        foreach (GameObject obj in attriObjects)
        {
            Destroy(obj); // �� ������Ʈ ����
        }
        if (prefabList.Count < 3 || spawnPoints.Count < 3)
        {
            Debug.LogError("There must be at least 3 prefabs and 3 spawn points.");
            return;
        }

        List<GameObject> shuffledPrefabs = new List<GameObject>(prefabList);
        ShuffleList(shuffledPrefabs); // ������ ����Ʈ�� �����Ͽ� �ߺ� ����

        for (int i = 0; i < 3; i++)
        {
            GameObject selectedPrefab = shuffledPrefabs[i];
            Transform spawnPoint = spawnPoints[i];

            GameObject spawnedPrefab = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation, panel.transform);
            spawnedPrefab.transform.localPosition = spawnPoint.localPosition;
        }
    }

    // ����Ʈ ���� �޼���
    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            GameObject temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
