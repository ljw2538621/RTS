using RTS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneRocks : ResourceBase
{
    protected GameObject[] m_ModelArr;

    void Awake()
    {
        base.BaseAwake();
        m_ModelArr = new GameObject[transform.Find("InitialModel").childCount];
        for (int i = 0; i < m_ModelArr.Length; i++)
        {
            m_ModelArr[i] = transform.Find("InitialModel").GetChild(i).gameObject;
        }

        m_Data.id = 3301;
        ResourceManagerData data = ResourceManager.GetInstance().GetDataById(m_Data.id);
        m_Data.name = data.name;
        m_Data.descripion = data.descripion;
        m_Data.icon = data.icon;
        m_Data.total = data.total;
        m_Data.store = m_Data.total;
        m_GetOnceNum = data.node.number;
        m_resourceNode = data.node;
    }

    void OnEnable()
    {
        base.BaseOnEnable();
        for (int i = 0; i < m_ModelArr.Length; i++)
        {
            m_ModelArr[i].SetActive(true);
        }
    }

    void Start()
    {
        base.BaseStart();
    }

    void Update()
    {
        base.BaseUpdate();
    }

    public override ResourceNode GetResourceNode()
    {
        if (m_Data.store < m_resourceNode.number)
        {
            m_resourceNode.number = m_Data.store;
        }
        m_Data.store -= m_resourceNode.number;
        float ratio = m_Data.store / m_Data.total;
        if (ratio > 0.5f && ratio < 0.75f)
        {
            m_ModelArr[3].SetActive(false);
        }
        else
        {
            if (ratio > 0.25 && ratio < 0.5f)
            {
                m_ModelArr[2].SetActive(false);
            }
            else
            {
                m_ModelArr[1].SetActive(false);
            }
        }

        if (m_Data.store <= 0)
        {
            gameObject.SetActive(false);
        }
        return m_resourceNode;
    }
}
