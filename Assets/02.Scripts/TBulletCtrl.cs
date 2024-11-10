using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TBulletCtrl : MonoBehaviour
{
    // �Ѿ� �߻� �ӵ�
    public float speed = 1000.0f;

    // ����ũ ��ƼŬ ������ ������ ����
    public GameObject sparkEffect;

    public GameObject Player;

    void Start()
    {
        Player = GameObject.Find("Player");
        speed = 3000.0f;

        if (gameObject.tag == "E_BULLET")  // ���Ͱ� �� �Ѿ��� �� 
        {
            // ���̵� 4������ 15�� �þ����... 3000���� ���� (�Ѿ� �̵��ӵ�)
            float a_CacSpeed = (GlobalValue.g_CurFloorNum - 3) * 15.0f;
            if (a_CacSpeed < 0.0f)
                a_CacSpeed = 0.0f;
            if (2200.0f < a_CacSpeed)
                a_CacSpeed = 2200.0f;

            speed = 800.0f + a_CacSpeed;
        }

        // Rigidbody�� ���� ������Ʈ�� ���� �������� ���� ���� �Ѿ��� �߻�
        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        // 4�� �� �ڵ� �ı�
        Destroy(gameObject, 4.0f);
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.name.Contains("Player") == true)
            return;

        if (coll.gameObject.name.Contains("Barrel") == true)
            return;

        if (coll.gameObject.name.Contains("Monster_") == true)
            return;

        if (coll.collider.tag == "SideWall")
            return;

        if (coll.collider.tag == "BULLET")
            return;

        if (coll.collider.tag == "E_BULLET")
            return;

        // ����ũ ��ƼŬ�� �������� ����
        GameObject spark = Instantiate(sparkEffect, transform.position, Quaternion.identity);
        // ParticleSystem ������Ʈ�� ����ð�(duration)�� ���� �� ���� ó��
        Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

        // �浹�� ���� ������Ʈ ����
        Destroy(gameObject);
    }
}
