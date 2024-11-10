using System.Collections.Generic;
using UnityEngine;

public class AttributeManager : MonoBehaviour
{
    public GameObject panel;                // 부모가 될 판넬 오브젝트
    public List<GameObject> prefabList;     // 12개의 프리팹 리스트
    public List<Transform> spawnPoints;     // 프리팹을 생성할 3개의 위치
 

    public void SpawnRandomPrefabs()
    {
        panel.SetActive(true);
        GameObject[] attriObjects = GameObject.FindGameObjectsWithTag("attri");


        foreach (GameObject obj in attriObjects)
        {
            Destroy(obj); // 각 오브젝트 삭제
        }
        if (prefabList.Count < 3 || spawnPoints.Count < 3)
        {
            Debug.LogError("There must be at least 3 prefabs and 3 spawn points.");
            return;
        }

        List<GameObject> shuffledPrefabs = new List<GameObject>(prefabList);
        ShuffleList(shuffledPrefabs); // 프리팹 리스트를 셔플하여 중복 방지

        for (int i = 0; i < 3; i++)
        {
            GameObject selectedPrefab = shuffledPrefabs[i];
            Transform spawnPoint = spawnPoints[i];

            GameObject spawnedPrefab = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation, panel.transform);
            spawnedPrefab.transform.localPosition = spawnPoint.localPosition;
        }
    }

    // 리스트 셔플 메서드
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
