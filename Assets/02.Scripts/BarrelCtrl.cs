using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCtrl : MonoBehaviour
{
    //폭발 효과 파티클 연결 변수
    public GameObject expEffect;
    //무작위로 선택할 텍스처 배열
    public Texture[] textures;

    //총알 맞은 회수를 누적시킬 변수
    private int hitCount = 0;
    float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < timer)
        {
            timer -= Time.deltaTime;
            if(timer <= 0.0f)
            {
                Rigidbody rbody = this.GetComponent<Rigidbody>();
                if (rbody != null)
                    rbody.mass = 20.0f;
            }
        }//if(0.0f < timer)

    }//void Update()

    //충돌 시 발생하는 콜백 함수(CallBack Function)
    void OnCollisionEnter(Collision coll)
    {
        if(coll.collider.tag == "BULLET")
        {
            //충돌한 총알 제거
            Destroy(coll.gameObject);

            //총알 맞은 횟수를 증가시키고 3회 이상이면 폭발 처리
            if (++hitCount >= 3)
                ExpBarrel();
        }
    }//void OnCollisionEnter(Collision coll)

    //드럼통 폭발시킬 함수
    void ExpBarrel()
    {
        //폭발 효과 파티클 생성
        GameObject explosion = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(explosion, explosion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        //지정한 원점을 중심으로 10.0f 반경 내에 들어와 있는 Collider객체 추출
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);
        BarrelCtrl a_Barrel = null;
        Rigidbody rbody = null;
        //추출한 Collider 객체에 폭발력 전달
        foreach(Collider coll in colls)
        {
            a_Barrel = coll.GetComponent<BarrelCtrl>();
            if (a_Barrel == null)
                continue;

            rbody = coll.GetComponent<Rigidbody>();
            if(rbody != null)
            {
                rbody.mass = 1.0f;
                rbody.AddExplosionForce(1000.0f, transform.position, 10.0f, 300.0f);
                a_Barrel.timer = 0.1f;
            }
        }

        //5초 후에 드럼통 제거
        Destroy(gameObject, 5.0f);
    }
}
