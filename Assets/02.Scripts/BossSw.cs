using UnityEngine;

public class BossSw : MonoBehaviour
{
    public GameObject bossPrefab; // ������ ������ (��: ���� ������Ʈ)
    public Transform spawnLocation; // �������� ������ ��ġ
    private float elapsedTime = 0f; // ��� �ð��� �����ϱ� ���� ����
    private bool hasSpawned = false; // �� ���� �����ǵ��� üũ�ϴ� ����

    void Update()
    {
        if (hasSpawned) return; // �̹� �����Ǿ����� �� �̻� �������� ����

        elapsedTime += Time.deltaTime; // �����Ӹ��� ��� �ð� ����

        if (elapsedTime >= 480f) // 480�� = 8��
        {
            Instantiate(bossPrefab, spawnLocation.position, spawnLocation.rotation); // ������ ����
            hasSpawned = true; // ���� �� �ٽ� �������� �ʵ��� ����
        }
    }
}
