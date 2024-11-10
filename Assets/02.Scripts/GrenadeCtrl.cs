using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeCtrl : MonoBehaviour
{
    // ���� ȿ�� ��ƼŬ ����
    public GameObject expEffect;

    // ���� ���� Ÿ�̸�
    float timer = 2.0f;

    // �������� ������ �ؽ��� �迭
    public Texture[] textures;

    // ����ź ���ư��� �ӵ�
    float speed = 500.0f;
    Vector3 m_ForwardDir = Vector3.zero;

    bool isRot = true;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];

        // ���ư��� ���� ������
        transform.forward = m_ForwardDir;
        transform.eulerAngles = new Vector3(20.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        GetComponent<Rigidbody>().AddForce(m_ForwardDir * speed);

        // AudioSource ������Ʈ �߰� �� ����
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Sounds �������� DM-CGS-48 ����� ���� ���� �ε�
        AudioClip explosionSound = Resources.Load<AudioClip>("DM-CGS-48");
        if (explosionSound != null)
        {
            audioSource.clip = explosionSound;
        }
        else
        {
            Debug.LogWarning("DM-CGS-48 sound file not found in Resources/Sounds folder.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f < timer)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                ExpGrenade();
            }
        }

        if (isRot == true)
        {
            transform.Rotate(new Vector3(Time.deltaTime * 190.0f, 0.0f, 0.0f), Space.Self);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        isRot = false;
    }

    void ExpGrenade()
    {
        // ���� ȿ�� ��ƼŬ ����
        GameObject explosion = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(explosion, explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        // ���� �Ҹ� ���
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // ������ ������ �߽����� 10.0f �ݰ� ���� �ִ� Collider ��ü ����
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);

        // ������ Collider ��ü�� ���߷� ����
        MonsterCtrl a_MonCtrl = null;
        foreach (Collider coll in colls)
        {
            a_MonCtrl = coll.GetComponent<MonsterCtrl>();
            if (a_MonCtrl == null)
                continue;

            a_MonCtrl.TakeDamage(150);
        }

        // ������Ʈ ���� (����� ��� �� ���� ���� �� ����)
        Destroy(gameObject, 0.5f);
    }

    public void SetForwardDir(Vector3 a_Dir)
    {
        m_ForwardDir = new Vector3(a_Dir.x, a_Dir.y + 0.5f, a_Dir.z);
    }
}