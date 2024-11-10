using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{

    //총알 발사 속도
    public float speed = 1000.0f;

    //스파크 파티클 프리팹 연결할 변수
    public GameObject sparkEffect;

    public GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Player");
      

        speed = 3000.0f;
        if (gameObject.tag == "E_BULLET")  //몬스터가 쏜 총알일 때 
        {
            //--- 난이도 4층부터 15씩 늘어나도록... 3000까지 증가 (총알 이동속도)
            float a_CacSpeed = (GlobalValue.g_CurFloorNum - 3) * 15.0f;
            if (a_CacSpeed < 0.0f)
                a_CacSpeed = 0.0f;
            if (2200.0f < a_CacSpeed)
                a_CacSpeed = 2200.0f;
            //--- 난이도 4층부터 15씩 늘어나도록... 3000까지 증가 (총알 이동속도)

            speed = 800.0f + a_CacSpeed;
        }
        else  //주인공이 쏜 총알일 때
        {
            if (Player.GetComponent<PlayerCtrl>().blood == true)
            {
                Renderer[] childRenderers = GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in childRenderers)
                {
                    renderer.material.color = Color.red;
                }
            }
            transform.forward = FollowCam.m_RifleDir.normalized;
        }

        GetComponent<Rigidbody>().AddForce(transform.forward * speed);

        Destroy(gameObject, 4.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
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

        //스파크 파티클을 동적으로 생성
        GameObject spark = Instantiate(sparkEffect, transform.position, Quaternion.identity);
        //ParticleSystem 컴포넌트의 수행시간(duration)이 지난 후 삭제 처리
        Destroy(spark, spark.GetComponent<ParticleSystem>().main.duration + 0.2f);

        //충돌한 게임오브젝트 삭제
        Destroy(gameObject);

    }//void OnCollisionEnter(Collision coll)
}
