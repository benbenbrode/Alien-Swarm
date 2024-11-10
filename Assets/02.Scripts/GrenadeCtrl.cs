using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeCtrl : MonoBehaviour
{
    // 폭발 효과 파티클 연결
    public GameObject expEffect;

    // 폭발 지연 타이머
    float timer = 2.0f;

    // 무작위로 선택할 텍스쳐 배열
    public Texture[] textures;

    // 수류탄 날아가는 속도
    float speed = 500.0f;
    Vector3 m_ForwardDir = Vector3.zero;

    bool isRot = true;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];

        // 날아가는 방향 재조정
        transform.forward = m_ForwardDir;
        transform.eulerAngles = new Vector3(20.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        GetComponent<Rigidbody>().AddForce(m_ForwardDir * speed);

        // AudioSource 컴포넌트 추가 및 설정
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Sounds 폴더에서 DM-CGS-48 오디오 파일 동적 로드
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
        // 폭발 효과 파티클 생성
        GameObject explosion = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(explosion, explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        // 폭발 소리 재생
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // 지정한 원점을 중심으로 10.0f 반경 내에 있는 Collider 객체 추출
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);

        // 추출한 Collider 객체에 폭발력 전달
        MonsterCtrl a_MonCtrl = null;
        foreach (Collider coll in colls)
        {
            a_MonCtrl = coll.GetComponent<MonsterCtrl>();
            if (a_MonCtrl == null)
                continue;

            a_MonCtrl.TakeDamage(150);
        }

        // 오브젝트 제거 (오디오 재생 후 조금 지연 후 제거)
        Destroy(gameObject, 0.5f);
    }

    public void SetForwardDir(Vector3 a_Dir)
    {
        m_ForwardDir = new Vector3(a_Dir.x, a_Dir.y + 0.5f, a_Dir.z);
    }
}
