using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkCool_NodeCtrl : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    float skill_Time = 0.0f;
    float skill_Delay = 0.0f;
    public Image time_Image = null;
    public Image icon_Image = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        skill_Time -= Time.deltaTime;
        time_Image.fillAmount = skill_Time / skill_Delay;

        if (skill_Time <= 0.0f)
            Destroy(gameObject);
    }

    public void InitState(float a_Time, float a_Delay)
    {
        skill_Time = a_Time;
        skill_Delay = a_Delay;
    }
}
