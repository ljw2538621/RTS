using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Library : BuildingBase
{
    protected GameObject m_LibraryMenu;
    protected Sprite m_ScholarIcon;
    protected int m_ScholarId;
    protected int m_ScholarCount;                       // 学者数量
    protected float m_PassTime;
    protected int m_ScholarMaxNum;
    protected int m_CreateScholarTime;
    protected int m_CreateBookOnceNum;
    protected float m_CreateBookTime;

    private void Awake()
    {
        base.BaseAwake();
        m_ModelHeight = 3.7f;
        m_Model = transform.Find("LibraryModel").gameObject;
    }

    private void OnEnable()
    {
        base.BaseOnEnable();
        m_PassTime = 0.0f;
    }

    private void Start()
    {
        base.BaseStart();
        m_ScholarMaxNum = (int)m_data.data1;
        m_ScholarId = (int)m_data.data2;
        m_CreateBookTime = (int)m_data.data3;
        m_CreateBookOnceNum = (int)m_data.data4;
        m_ScholarIcon = UnitManager.GetInstance().GetSpriteById(m_ScholarId);
        m_LibraryMenu = GameObject.Find("UiCanvas/PlayerView/FunctionMenu/LibraryMenu");
    }

    private void Update()
    {
        base.BaseUpdate();
    }

    public override void OpenFunctionMenu()
    {
        if (m_State == BuildingState.BS_NORMAL)
        {
            m_LibraryMenu.GetComponent<CreateUnitMenuBase>().OpenMenu(gameObject);
        }
    }

    public override void BuildingNormalActionUpdate()
    {
        if (m_ScholarCount > 0)
        {
            m_PassTime += Time.deltaTime;
            if (m_PassTime > m_CreateBookTime)
            {
                m_PassTime -= m_CreateBookTime;
                m_Master.GetComponent<MasterBase>().AddResource(0, 0, 0, m_ScholarCount * m_CreateBookOnceNum);
            }
        }

        if (m_CreateList.Count > 0)
        {
            UnitNode node = m_CreateList[0];
            node.passTime += Time.deltaTime;
            if (node.passTime >= node.finishTime)
            {
                ++m_ScholarCount;
                m_CreateList.RemoveAt(0);
            }
            else
            {
                m_CreateList[0] = node;
            }
        }
    }

    public override void BeSelect(bool isOpen, GameObject user)
    {
        if (isOpen)
        {
            m_Plane.SetActive(true);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(true);
                m_MessageMenu.GetComponent<MessageMenu>().SetMessage(gameObject, Message_Type.MT_BUILDING);
            }
        }
        else
        {
            m_Plane.SetActive(false);
            if (user == m_Master)
            {
                m_MessageMenu.SetActive(false);
            }
        }
    }

    public override int AddCreateUnitNode(int id)
    {
        return -1;
    }
}
