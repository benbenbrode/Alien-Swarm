using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkInvenNode : MonoBehaviour
{
    [HideInInspector] public SkillType m_SkType;
    [HideInInspector] public Text m_SkCountText;    //스킬 카운트 텍스트 

    // Start is called before the first frame update
    void Start()
    {
       Button a_BtnCom = this.GetComponent<Button>();
        if (a_BtnCom != null)
            a_BtnCom.onClick.AddListener(() =>
            {   //이 버튼을 눌렀을 때

                if (GlobalValue.g_SkillCount[(int)m_SkType] <= 0)
                    return;

                PlayerCtrl a_Player = GameObject.FindObjectOfType<PlayerCtrl>();
                if (a_Player != null)
                    a_Player.UseSkill_Item(m_SkType);

                if (m_SkCountText != null)
                    m_SkCountText.text = GlobalValue.g_SkillCount[(int)m_SkType].ToString();
            }); 
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void InitState(SkillType a_SkType)
    {
        m_SkType = a_SkType;
        m_SkCountText = GetComponentInChildren<Text>();
        if (m_SkCountText != null)
            m_SkCountText.text = GlobalValue.g_SkillCount[(int)m_SkType].ToString();
    }
}
