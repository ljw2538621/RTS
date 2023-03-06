using RTS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Message_Type
{
    MT_NONE,
    MT_UNIT,
    MT_BUILDING,
    MT_RESOURCE
}

public class MessageMenu : MonoBehaviour
{
    protected GameObject m_Object;
    protected Image m_HeadImage;
    protected Text m_Name;
    protected Image m_Blood;
    protected Text m_BloodValue;
    protected Text m_Message;
    protected Message_Type m_Type;

    void Awake()
    {
        m_HeadImage = transform.Find("HeadImage").GetComponent<Image>();
        m_Name = transform.Find("Name").GetComponent<Text>();
        m_Blood = transform.Find("FullBlood/Blood").GetComponent<Image>();
        m_BloodValue = transform.Find("FullBlood/Value").GetComponent<Text>();
        m_Message = transform.Find("Message").GetComponent<Text>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        switch (m_Type)
        {
            case Message_Type.MT_NONE:
            break;
            case Message_Type.MT_UNIT:
            {
                UnitData data = m_Object.GetComponent<UnitBase>().m_Data;
                m_Blood.fillAmount = data.hp / data.maxhp;
                m_BloodValue.text = ((int)data.hp).ToString();
            }
            break;
            case Message_Type.MT_BUILDING:
            {
                BuildingData data = m_Object.GetComponent<BuildingBase>().GetBuildingData();
                m_Blood.fillAmount = data.hp / data.maxhp;
                m_BloodValue.text = ((int)data.hp).ToString();
            }
            break;
            case Message_Type.MT_RESOURCE:
            {
                ResourceData data = m_Object.GetComponent<ResourceBase>().GetData();
                m_Blood.fillAmount = (float)data.store / (float)data.total;
                m_BloodValue.text = data.store.ToString();
            }
            break;
        }
    }

    public void SetMessage(GameObject other, Message_Type type)
    {
        m_Object = other;
        m_Type = type;
        switch (type)
        {
            case Message_Type.MT_NONE:
            {
                m_Object = null;
            }
            break;
            case Message_Type.MT_UNIT:
            {
                UnitData data = m_Object.GetComponent<UnitBase>().m_Data;
                m_HeadImage.sprite = data.icon;
                m_Name.text = data.name;
                m_Blood.fillAmount = data.hp / data.maxhp;
                m_BloodValue.text = ((int)data.hp).ToString();
                m_Message.text = data.descripion;
            }
            break;
            case Message_Type.MT_BUILDING:
            {
                BuildingData data = m_Object.GetComponent<BuildingBase>().GetBuildingData();
                m_HeadImage.sprite = data.icon;
                m_Name.text = data.name;
                m_Blood.fillAmount = data.hp / data.maxhp;
                m_BloodValue.text = ((int)data.hp).ToString();
                m_Message.text = data.descripion;
            }
            break;
            case Message_Type.MT_RESOURCE:
            {
                ResourceData data = m_Object.GetComponent<ResourceBase>().GetData();
                m_HeadImage.sprite = data.icon;
                m_Name.text = data.name;
                m_Blood.fillAmount = data.store / data.total;
                m_BloodValue.text = data.store.ToString();
                m_Message.text = data.descripion;
            }
            break;
        }
    }
}
