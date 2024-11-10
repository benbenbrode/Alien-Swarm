using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject projectilePrefab;   // �߻��� ������
    public float fireRange = 100f;         // ���� ����
    public Transform firePoint;           // �߻� ��ġ

    private float fireTimer;
    public GameObject Player;

    private void Start()
    {
        Player = GameObject.Find("Player");


        // Resources �������� TBullet �������� �ε�
        projectilePrefab = Resources.Load<GameObject>("TBullet");

        // �ڽ� ������Ʈ �� firepos��� �̸��� ������Ʈ�� firePoint�� ����
        Transform foundFirePoint = transform.Find("firepos");
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
        }
        else
        {
            Debug.LogWarning("firepos��� �̸��� �ڽ� ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= Player.GetComponent<PlayerCtrl>().Tfirespeed)
        {
            GameObject target = FindClosestMonster();
            if (target != null)
            {
                RotateTowardsTarget(target);
                FireProjectile();
            }
            fireTimer = 0f;
        }
    }

    GameObject FindClosestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        GameObject closestMonster = null;
        float shortestDistance = fireRange;

        foreach (GameObject monster in monsters)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
            if (distanceToMonster < shortestDistance)
            {
                shortestDistance = distanceToMonster;
                closestMonster = monster;
            }
        }
        return closestMonster;
    }

    void RotateTowardsTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;  // Y�� ȸ���� ���

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }

    void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // ���� �ͷ��� Y ȸ���� �����ͼ� firePoint�� ȸ���� ����
            Quaternion projectileRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        }
    }
}
