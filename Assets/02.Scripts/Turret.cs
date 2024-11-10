using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject projectilePrefab;   // 발사할 프리팹
    public float fireRange = 100f;         // 공격 범위
    public Transform firePoint;           // 발사 위치

    private float fireTimer;
    public GameObject Player;

    private void Start()
    {
        Player = GameObject.Find("Player");


        // Resources 폴더에서 TBullet 프리팹을 로드
        projectilePrefab = Resources.Load<GameObject>("TBullet");

        // 자식 오브젝트 중 firepos라는 이름의 오브젝트를 firePoint로 설정
        Transform foundFirePoint = transform.Find("firepos");
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
        }
        else
        {
            Debug.LogWarning("firepos라는 이름의 자식 오브젝트를 찾을 수 없습니다.");
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
        direction.y = 0;  // Y축 회전만 고려

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }

    void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // 현재 터렛의 Y 회전을 가져와서 firePoint의 회전에 적용
            Quaternion projectileRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        }
    }
}
